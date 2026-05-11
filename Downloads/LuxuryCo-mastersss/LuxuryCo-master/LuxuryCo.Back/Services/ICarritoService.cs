using LuxuryCo.Back.DTOs;

namespace LuxuryCo.Back.Services;

public interface ICarritoService
{
    Task<CarritoDto> GetOrCreateCartAsync(int idUsuario);
    Task<bool> AddToCartAsync(int idUsuario, int idProducto, int cantidad, string? talla = null, string? color = null);
    Task<bool> RemoveFromCartAsync(int idUsuario, int idDetalleCarrito);
    Task<bool> UpdateQuantityAsync(int idUsuario, int idProducto, int cantidad, string? talla = null, string? color = null);
    Task<bool> ClearCartAsync(int idUsuario);
}
