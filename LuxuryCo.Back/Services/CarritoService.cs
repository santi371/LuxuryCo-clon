using LuxuryCo.Back.DTOs;
using LuxuryCo.Database.Data;
using LuxuryCo.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace LuxuryCo.Back.Services;

public class CarritoService : ICarritoService
{
    private readonly LuxuryCoDbContext _context;

    public CarritoService(LuxuryCoDbContext context)
    {
        _context = context;
    }

    private async Task<Carrito> InternalGetOrCreateCartAsync(int idUsuario)
    {
        var carrito = await _context.Carritos
            .Include(c => c.Detalles)
                .ThenInclude(d => d.Producto)
                    .ThenInclude(p => p.Imagenes.Where(i => i.principal == true))
            .Include(c => c.Detalles)
                .ThenInclude(d => d.Producto)
                    .ThenInclude(p => p.Marca)
            .FirstOrDefaultAsync(c => c.id_usuario == idUsuario);

        if (carrito == null)
        {
            carrito = new Carrito
            {
                id_usuario = idUsuario,
                fecha_creacion = DateTime.UtcNow
            };
            _context.Carritos.Add(carrito);
            await _context.SaveChangesAsync();
        }

        return carrito;
    }

    private CarritoDto MapToDto(Carrito carrito)
    {
        return new CarritoDto
        {
            IdCarrito = carrito.id_carrito,
            IdUsuario = carrito.id_usuario,
            FechaCreacion = carrito.fecha_creacion,
            Detalles = carrito.Detalles.Select(d => new DetalleCarritoDto
            {
                IdDetalleCarrito = d.id_detalle_carrito,
                IdProducto = d.id_producto,
                NombreProducto = d.Producto?.nombre ?? "Desconocido",
                MarcaNombre = d.Producto?.Marca?.nombre,
                ImagenUrl = d.Producto?.Imagenes.FirstOrDefault()?.url_imagen,
                PrecioUnitario = d.Producto?.precio ?? 0,
                Cantidad = d.cantidad
            }).ToList()
        };
    }

    public async Task<CarritoDto> GetOrCreateCartAsync(int idUsuario)
    {
        var carrito = await InternalGetOrCreateCartAsync(idUsuario);
        return MapToDto(carrito);
    }

    public async Task<bool> AddToCartAsync(int idUsuario, int idProducto, int cantidad)
    {
        if (cantidad <= 0) throw new ArgumentException("La cantidad debe ser mayor a cero.");

        var carrito = await InternalGetOrCreateCartAsync(idUsuario);
        
        var producto = await _context.Productos.FirstOrDefaultAsync(p => p.id_producto == idProducto && p.activo);
        if (producto == null) throw new Exception("El producto no existe o está inactivo.");

        var detalleExistente = carrito.Detalles.FirstOrDefault(d => d.id_producto == idProducto);
        int nuevaCantidad = (detalleExistente?.cantidad ?? 0) + cantidad;

        // Validación CRÍTICA contra Inventario
        if (nuevaCantidad > producto.stock)
        {
            throw new Exception($"Stock insuficiente. Hay {producto.stock} en inventario y estás intentando añadir {nuevaCantidad}.");
        }

        if (detalleExistente != null)
        {
            detalleExistente.cantidad = nuevaCantidad;
        }
        else
        {
            var nuevoDetalle = new DetalleCarrito
            {
                id_carrito = carrito.id_carrito,
                id_producto = idProducto,
                cantidad = cantidad
            };
            _context.DetallesCarrito.Add(nuevoDetalle);
        }

        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> RemoveFromCartAsync(int idUsuario, int idDetalleCarrito)
    {
        var carrito = await InternalGetOrCreateCartAsync(idUsuario);
        var detalle = carrito.Detalles.FirstOrDefault(d => d.id_detalle_carrito == idDetalleCarrito);

        if (detalle == null) return false;

        _context.DetallesCarrito.Remove(detalle);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> UpdateQuantityAsync(int idUsuario, int idProducto, int cantidad)
    {
        var carrito = await InternalGetOrCreateCartAsync(idUsuario);
        var detalle = carrito.Detalles.FirstOrDefault(d => d.id_producto == idProducto);

        if (detalle == null) 
        {
            if (cantidad > 0) return await AddToCartAsync(idUsuario, idProducto, cantidad);
            return false;
        }

        if (cantidad <= 0) 
        {
            _context.DetallesCarrito.Remove(detalle);
            return await _context.SaveChangesAsync() > 0;
        }

        var producto = await _context.Productos.FirstOrDefaultAsync(p => p.id_producto == idProducto && p.activo);
        if (producto == null) throw new Exception("El producto no existe.");

        if (cantidad > producto.stock)
        {
            throw new Exception($"Stock insuficiente. Solo quedan {producto.stock} unidades.");
        }

        detalle.cantidad = cantidad;
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> ClearCartAsync(int idUsuario)
    {
        var carrito = await InternalGetOrCreateCartAsync(idUsuario);
        if (!carrito.Detalles.Any()) return true;

        _context.DetallesCarrito.RemoveRange(carrito.Detalles);
        return await _context.SaveChangesAsync() > 0;
    }
}
