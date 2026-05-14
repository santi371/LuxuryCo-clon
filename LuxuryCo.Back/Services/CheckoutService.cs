using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LuxuryCo.Back.DTOs;
using LuxuryCo.Database.Data;
using LuxuryCo.Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LuxuryCo.Back.Services;

public class CheckoutService : ICheckoutService
{
    private readonly LuxuryCoDbContext _context;
    private readonly IPaymentGatewayService _paymentGateway;
    private readonly ILogger<CheckoutService> _logger;

    public CheckoutService(LuxuryCoDbContext context, IPaymentGatewayService paymentGateway, ILogger<CheckoutService> logger)
    {
        _context = context;
        _paymentGateway = paymentGateway;
        _logger = logger;
    }

    public async Task<CheckoutResponseDto> PlaceOrderAsync(int idUsuario, CheckoutRequestDto request)
    {
        _logger.LogInformation($"Iniciando proceso Checkout para Usuario {idUsuario} con Token de Idempotencia: {request.IdempotencyKey}");

        using var transaction = await _context.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);

        try
        {
            var carrito = await _context.Carritos
                .Include(c => c.Detalles)
                .ThenInclude(d => d.Producto)
                .FirstOrDefaultAsync(c => c.id_usuario == idUsuario);

            if (carrito == null || !carrito.Detalles.Any())
            {
                throw new InvalidOperationException("El carrito se encuentra vacío o no existe.");
            }

            decimal subtotal = 0;
            var orderDetails = new List<DetallePedido>();

            foreach (var item in carrito.Detalles)
            {
                var producto = await _context.Productos.FirstOrDefaultAsync(p => p.id_producto == item.id_producto);
                if (producto == null || !producto.activo) 
                    throw new Exception($"El producto '{item.Producto?.nombre}' ya no está disponible.");

                if (item.cantidad > producto.stock)
                {
                    throw new Exception($"Stock insuficiente para '{producto.nombre}'. Restan {producto.stock} unidades y has solicitado {item.cantidad}.");
                }

                // Conservar nullability properties
                int cantidadItem = item.cantidad;

                producto.stock -= cantidadItem;

                var detalleOrden = new DetallePedido
                {
                    id_producto = producto.id_producto,
                    cantidad = cantidadItem,
                    precio_unitario = producto.precio
                };
                
                subtotal += (producto.precio * cantidadItem);
                orderDetails.Add(detalleOrden);

                var inventarioSede = await _context.InventariosSede.FirstOrDefaultAsync(i => i.id_producto == producto.id_producto && i.id_sede == 1);
                if (inventarioSede != null)
                {
                    inventarioSede.cantidad_disponible -= cantidadItem;
                }
            }

            decimal costoEnvio = subtotal > 500000 ? 0 : 25000;
            decimal iva = subtotal * 0.19m; 
            decimal totalOrder = subtotal + iva + costoEnvio; 

            // Cargar una ciudad predeterminada (Bogotá) o la especificada 
            var ciudadDb = await _context.Ciudades.FirstOrDefaultAsync() ?? new Ciudad { id_ciudad = 1 };

            var dirObj = new DireccionUsuario
            {
                id_usuario = idUsuario,
                direccion = $"{request.DireccionLinea1}, {request.Ciudad}, CP: {request.CodigoPostal}, Tel: {request.Telefono}",
                referencia = request.Referencias,
                id_ciudad = ciudadDb.id_ciudad
            };
            
            _context.DireccionesUsuario.Add(dirObj);
            await _context.SaveChangesAsync();

            var pedido = new Pedido
            {
                id_usuario = idUsuario,
                id_direccion = dirObj.id_direccion,
                fecha_pedido = DateTime.UtcNow,
                total = totalOrder,
                id_estado_pedido = 1 
            };
            
            _context.Pedidos.Add(pedido);
            await _context.SaveChangesAsync(); 
            
            foreach (var detail in orderDetails)
            {
                detail.id_pedido = pedido.id_pedido;
                _context.DetallesPedido.Add(detail);
            }
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Contactando Gateway de Pago. Monto: {totalOrder} USD...");
            var paymentResult = await _paymentGateway.ProcessPaymentAsync(request.CardToken, totalOrder);

            if (!paymentResult.Success)
            {
                _logger.LogWarning($"[PAYMENT REJECTED] Fondos insuficientes. Orden {pedido.id_pedido}. Ejecutando ROLLBACK...");
                throw new Exception("GATEWAY_REJECTED: La transacción fue declinada por su red bancaria. No se han hecho cobros.");
            }

            pedido.id_estado_pedido = 2; 
            _logger.LogInformation($"[PAYMENT APPROVED] Transacción {paymentResult.TransactionId} exitosa para la orden {pedido.id_pedido}.");

            // --- Generar Factura de la compra ---
            var factura = new Factura
            {
                id_pedido = pedido.id_pedido,
                fecha_factura = DateTime.UtcNow,
                total = totalOrder,
                id_metodo_pago = 1 // 1 = Tarjeta de Crédito por defecto
            };
            _context.Facturas.Add(factura);

            _context.DetallesCarrito.RemoveRange(carrito.Detalles);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return new CheckoutResponseDto
            {
                Success = true,
                Message = "¡Orden procesada con éxito!",
                OrderId = pedido.id_pedido,
                TrackingNumber = paymentResult.TransactionId
            };
        }
        catch (DbUpdateConcurrencyException dbEx)
        {
            await transaction.RollbackAsync();
            _logger.LogError($"[ERROR CONCURRENCIA] Race Condition detectada: {dbEx.Message}");
            return new CheckoutResponseDto { Success = false, Message = "Stock agotado por otra compra paralela." };
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError($"[ERROR CHECKOUT] Flow interrumpido: {ex.Message}");
            return new CheckoutResponseDto { Success = false, Message = ex.Message.Contains("GATEWAY_REJECTED") ? "Pago declinado. Tu tarjeta no fue aprobada." : ex.Message };
        }
    }
}
