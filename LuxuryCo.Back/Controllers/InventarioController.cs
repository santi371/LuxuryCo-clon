using LuxuryCo.Back.DTOs;
using LuxuryCo.Back.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LuxuryCo.Back.Controllers;

[Route("api/[controller]")]
[ApiController]
public class InventarioController : ControllerBase
{
    private readonly IInventarioService _inventarioService;

    public InventarioController(IInventarioService inventarioService)
    {
        _inventarioService = inventarioService;
    }

    // GET api/inventario?idSede=1
    [HttpGet]
    public async Task<IActionResult> GetResumen([FromQuery] int? idSede = null)
    {
        var resumen = await _inventarioService.GetResumenAsync(idSede);
        return Ok(resumen);
    }

    // GET api/inventario/bajo-stock
    [HttpGet("bajo-stock")]
    public async Task<IActionResult> GetBajoStock()
    {
        var alertas = await _inventarioService.GetBajoStockAsync();
        return Ok(alertas);
    }

    // PUT api/inventario/ajustar
    [Authorize(Roles = "ADMIN,SUPERVISOR")]
    [HttpPut("ajustar")]
    public async Task<IActionResult> AjustarStock([FromBody] InventarioAjusteDto dto)
    {
        try
        {
            var usuarioIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            int usuarioId = int.TryParse(usuarioIdStr, out var uid) ? uid : 0;

            var resultado = await _inventarioService.AjustarStockAsync(dto, usuarioId);
            return Ok(resultado);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    // GET api/inventario/historial?idSede=1&idProducto=5
    [HttpGet("historial")]
    public async Task<IActionResult> GetHistorial([FromQuery] int? idSede = null, [FromQuery] int? idProducto = null)
    {
        var historial = await _inventarioService.GetHistorialAsync(idSede, idProducto);
        return Ok(historial);
    }

    // POST api/inventario/entrada
    [Authorize(Roles = "ADMIN,SUPERVISOR")]
    [HttpPost("entrada")]
    public async Task<IActionResult> RegistrarEntrada([FromBody] AbastecimientoCreateDto dto)
    {
        try
        {
            var usuarioIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            int usuarioId = int.TryParse(usuarioIdStr, out var uid) ? uid : 0;

            var resultado = await _inventarioService.RegistrarEntradaAsync(dto, usuarioId);
            return Ok(resultado);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }
}
