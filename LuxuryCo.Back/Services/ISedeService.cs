using LuxuryCo.Back.DTOs;

namespace LuxuryCo.Back.Services;

public interface ISedeService
{
    Task<IEnumerable<SedeDto>> GetAllAsync();
    Task<SedeDto> GetByIdAsync(int id);
    Task<SedeDto> CreateAsync(SedeCreateUpdateDto createDto);
    Task<SedeDto> UpdateAsync(int id, SedeCreateUpdateDto updateDto);
    Task<bool> ToggleStateAsync(int id);
}
