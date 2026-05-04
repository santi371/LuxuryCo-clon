using LuxuryCo.Back.DTOs;

namespace LuxuryCo.Back.Services;

public interface IProductoService
{
    Task<IEnumerable<ProductoDto>> GetAllAsync(bool adminView = false);
    Task<ProductoDto> GetByIdAsync(int id);
    Task<ProductoDto> CreateAsync(ProductoCreateUpdateDto createDto);
    Task<ProductoDto> UpdateAsync(int id, ProductoCreateUpdateDto updateDto);
    Task<IEnumerable<ProductoDto>> GetByCategoryAsync(string categoryName, bool adminView = false);
    Task<IEnumerable<ProductoDto>> SearchAsync(string query, bool adminView = false);
    Task<bool> DeleteAsync(int id);
    Task<bool> ToggleStateAsync(int id);
    Task<bool> UploadImagesAsync(int idProducto, List<IFormFile> imagenes);
    Task<bool> DeleteImageAsync(int idProducto, int idImagen);
    Task<bool> SetPrincipalImageAsync(int idProducto, int idImagen);
}
