using LuxuryCo.Back.DTOs;

namespace LuxuryCo.Back.Services;

public interface IInventarioService
{
    Task<IEnumerable<InventarioSedeDto>> GetResumenAsync(int? idSede = null);
    Task<IEnumerable<InventarioSedeDto>> GetBajoStockAsync();
    Task<InventarioSedeDto> AjustarStockAsync(InventarioAjusteDto dto, int idUsuario);
    Task<IEnumerable<AbastecimientoDto>> GetHistorialAsync(int? idSede = null, int? idProducto = null);
    Task<AbastecimientoDto> RegistrarEntradaAsync(AbastecimientoCreateDto dto, int idUsuario);
}
