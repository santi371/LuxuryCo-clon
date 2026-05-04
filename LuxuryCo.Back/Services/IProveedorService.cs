using LuxuryCo.Back.DTOs;

namespace LuxuryCo.Back.Services;

public interface IProveedorService
{
    Task<IEnumerable<ProveedorDto>> GetAllAsync();
    Task<ProveedorDto?> GetByIdAsync(int id);
    Task<ProveedorDto> CreateAsync(ProveedorCreateUpdateDto dto);
    Task<ProveedorDto> UpdateAsync(int id, ProveedorCreateUpdateDto dto);
    Task<bool> ToggleActivoAsync(int id);
    Task<bool> DeleteAsync(int id);
}
