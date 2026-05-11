using LuxuryCo.Back.DTOs;
using LuxuryCo.Back.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LuxuryCo.Back.Controllers;

[Route("api/[controller]")]
[ApiController]
public class SedeController : ControllerBase
{
    private readonly ISedeService _sedeService;

    public SedeController(ISedeService sedeService)
    {
        _sedeService = sedeService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var sedes = await _sedeService.GetAllAsync();
        return Ok(sedes);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var sede = await _sedeService.GetByIdAsync(id);
            return Ok(sede);
        }
        catch (Exception ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] SedeCreateUpdateDto dto)
    {
        try
        {
            var sede = await _sedeService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = sede.IdSede }, sede);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] SedeCreateUpdateDto dto)
    {
        try
        {
            var sede = await _sedeService.UpdateAsync(id, dto);
            return Ok(sede);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id}/toggle-status")]
    public async Task<IActionResult> ToggleStatus(int id)
    {
        try
        {
            await _sedeService.ToggleStateAsync(id);
            return Ok(new { message = "Estado de sede modificado." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
