using LuxuryCo.Back.DTOs;

namespace LuxuryCo.Back.Services;

public interface ICarritoService
{
    Task<CarritoDto> GetOrCreateCartAsync(int idUsuario);
    Task<bool> AddToCartAsync(int idUsuario, int idProducto, int cantidad);
    Task<bool> RemoveFromCartAsync(int idUsuario, int idDetalleCarrito);
    // Cambiamos a idProducto en vez de idDetalleCarrito para facilitar consumos frontend
    Task<bool> UpdateQuantityAsync(int idUsuario, int idProducto, int cantidad);
    Task<bool> ClearCartAsync(int idUsuario);
}
