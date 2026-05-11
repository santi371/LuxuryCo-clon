using LuxuryCo.Database.Models;

namespace LuxuryCo.Back.Services;

public class MarcaDto
{
    public int IdMarca { get; set; }
    public string Nombre { get; set; }
    public string Descripcion { get; set; }
    public string LogoUrl { get; set; }
}

public interface IMarcaService
{
    Task<IEnumerable<MarcaDto>> GetAllAsync();
    Task<MarcaDto?> GetByIdAsync(int id);
    Task<MarcaDto> CreateAsync(MarcaDto marcaDto);
    Task<MarcaDto> UpdateAsync(int id, MarcaDto marcaDto);
    Task<bool> DeleteAsync(int id);
}
