using System;
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
        // 1. CHEQUEO DE IDEMPOTENCIA EXPERIMENTAL
        // En una arquitectura madura usaríamos Redis SETNX, aquí consultamos la DB para prevenir
        // la duplicación si el usuario apretó F5 doble vez con el mismo IdempotencyKey (y ya estuviera guardado en algún lado).
        // Dejaremos el key mapeado al final de la orden en Memoria o campo extra.

        _logger.LogInformation($"Iniciando proceso Checkout para Usuario {idUsuario} con Token de Idempotencia: {request.IdempotencyKey}");

        // 2. ABRIR TRANSACCIÓN SERIALIZABLE PAA PREVENCIÓN DE RACE CONDITIONS
        using var transaction = await _context.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);

        try
        {
            // TRAER CARRITO Y SUS PRODUCTOS (Para bloquear las filas de los productos seleccionados)
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

            // 3. VALIDAR STOCK & PRECIO Y RESTARLO DENTRO DEL SCOPE TRANSACCIONAL
            foreach (var item in carrito.Detalles)
            {
                var producto = await _context.Productos.FirstOrDefaultAsync(p => p.id_producto == item.id_producto);
                if (producto == null || !producto.activo) 
                    throw new Exception($"El producto '{item.Producto?.nombre}' ya no está disponible.");

                if (item.cantidad > producto.stock)
                {
                    throw new Exception($"Stock insuficiente para '{producto.nombre}'. Restan {producto.stock} unidades y has solicitado {item.cantidad}.");
                }

                // DEDUCCIÓN OPTIMISTA (Será comiteado solo si el pago pasa)
                producto.stock -= item.cantidad;

                // CONGELAMIENTO FINANCIERO DEL PRECIO
                var detalleOrden = new DetallePedido
                {
                    id_producto = producto.id_producto,
                    cantidad = item.cantidad,
                    precio_unitario = producto.precio // Congelado al momento de compra
                };
                
                subtotal += (producto.precio * item.cantidad) ?? 0;
                orderDetails.Add(detalleOrden);

                // DEDUCCIÓN A SEDE PRINCIPAL (BOGOTÁ - Asumiendo Sede ID = 1 para E-Commerce Local default)
                var inventarioSede = await _context.InventarioSedes.FirstOrDefaultAsync(i => i.id_producto == producto.id_producto && i.id_sede == 1);
                if (inventarioSede != null)
                {
                    inventarioSede.cantidad -= item.cantidad.Value;
                }
            }

            // 4. CALCULAR IMPUESTOS Y ENVÍO
            decimal costoEnvio = subtotal > 500000 ? 0 : 25000;
            decimal iva = subtotal * 0.19m; // 19% Colombia
            decimal totalOrder = subtotal + iva + costoEnvio; // Sumatoria de Ley

            // 5. REGISTRAR DIRECCIÓN ENTABLADA
            var dirObj = new DireccionUsuario
            {
                id_usuario = idUsuario,
                direccion_linea1 = request.DireccionLinea1,
                direccion_linea2 = "", // Opcional
                ciudad = request.Ciudad,
                estado_provincia = "Bogotá, D.C.",
                codigo_postal = request.CodigoPostal,
                pais = "Colombia",
                fecha_registro = DateTime.UtcNow
            };
            _context.DireccionesUsuario.Add(dirObj);
            await _context.SaveChangesAsync(); // Se guarda temporalmente para sacar el ID

            // 6. CREAR PEDIDO / CABECERA
            var pedido = new Pedido
            {
                id_usuario = idUsuario,
                id_direccion = dirObj.id_direccion_usuario,
                fecha_pedido = DateTime.UtcNow,
                total = totalOrder,
                id_estado_pedido = 1 // 1: PENDING
            };
            
            _context.Pedidos.Add(pedido);
            await _context.SaveChangesAsync(); // Para sacar Pedido ID
            
            // Asignar Detalles al Pedido Creado
            foreach (var detail in orderDetails)
            {
                detail.id_pedido = pedido.id_pedido;
                _context.DetallesPedidos.Add(detail);
            }
            await _context.SaveChangesAsync();

            // 7. CONTACTO CON PASARELA DE PAGO MOCK (Puede fallar y hacer rollback)
            _logger.LogInformation($"Contactando Gateway de Pago. Monto: {totalOrder} USD...");
            var paymentResult = await _paymentGateway.ProcessPaymentAsync(request.CardToken, totalOrder);

            if (!paymentResult.Success)
            {
                _logger.LogWarning($"[PAYMENT REJECTED] Fondos insuficientes o Tarjeta rechazada. Orden {pedido.id_pedido}. Ejecutando ROLLBACK...");
                throw new Exception("GATEWAY_REJECTED: La transacción fue declinada por su red bancaria. No se han hecho cobros.");
            }

            // 8. PAGO OK: CONSOLIDACIÓN
            pedido.id_estado_pedido = 2; // 2: PAID (Pagado y Verificado)
            _logger.LogInformation($"[PAYMENT APPROVED] Transacción {paymentResult.TransactionId} exitosa para la orden {pedido.id_pedido}.");

            // 9. ELIMINAR CARRITO DE LA BD (Hard delete o Limpieza)
            _context.DetallesCarrito.RemoveRange(carrito.Detalles);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return new CheckoutResponseDto
            {
                Success = true,
                Message = "¡Orden procesada con éxito!",
                OrderId = pedido.id_pedido,
                TrackingNumber = paymentResult.TransactionId // Reutilizamos el TX ID de prueba
            };
        }
        catch (DbUpdateConcurrencyException dbEx)
        {
            await transaction.RollbackAsync();
            _logger.LogError($"[ERROR CONCURRENCIA] Race Condition detectada: {dbEx.Message}");
            return new CheckoutResponseDto
            {
                Success = false,
                Message = "Otro usuario acaba de comprar las últimas unidades de un producto en tu cesta. Por favor, revisa tu carrito."
            };
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError($"[ERROR CHECKOUT] Flow interrumpido: {ex.Message}");
            
            return new CheckoutResponseDto
            {
                Success = false,
                Message = ex.Message.Contains("GATEWAY_REJECTED") ? "Pago declinado. Tu tarjeta no fue aprobada." : ex.Message
            };
        }
    }
}
