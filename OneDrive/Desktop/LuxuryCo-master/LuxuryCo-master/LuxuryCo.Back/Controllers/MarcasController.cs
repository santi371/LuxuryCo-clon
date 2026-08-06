using LuxuryCo.Back.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LuxuryCo.Back.Controllers;

[Route("api/[controller]")]
[ApiController]
public class MarcasController : ControllerBase
{
    private readonly IMarcaService _marcaService;

    public MarcasController(IMarcaService marcaService)
    {
        _marcaService = marcaService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var marcas = await _marcaService.GetAllAsync();
        return Ok(marcas);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var marca = await _marcaService.GetByIdAsync(id);
        if (marca == null) return NotFound();
        return Ok(marca);
    }

    [HttpPost]
    // [Authorize(Roles = "ADMIN")] // Descomentar al asegurar el flujo total
    public async Task<IActionResult> Create(MarcaDto marcaDto)
    {
        var marca = await _marcaService.CreateAsync(marcaDto);
        return CreatedAtAction(nameof(GetById), new { id = marca.IdMarca }, marca);
    }

    [HttpPut("{id}")]
    // [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> Update(int id, MarcaDto marcaDto)
    {
        try
        {
            var marca = await _marcaService.UpdateAsync(id, marcaDto);
            return Ok(marca);
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    // [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _marcaService.DeleteAsync(id);
        if (!result) return NotFound();
        return NoContent();
    }
}
