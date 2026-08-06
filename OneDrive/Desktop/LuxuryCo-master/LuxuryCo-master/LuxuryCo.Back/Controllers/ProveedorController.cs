using LuxuryCo.Back.DTOs;
using LuxuryCo.Back.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LuxuryCo.Back.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ProveedorController : ControllerBase
{
    private readonly IProveedorService _proveedorService;

    public ProveedorController(IProveedorService proveedorService)
    {
        _proveedorService = proveedorService;
    }

    // GET api/proveedor
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var proveedores = await _proveedorService.GetAllAsync();
        return Ok(proveedores);
    }

    // GET api/proveedor/5
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var proveedor = await _proveedorService.GetByIdAsync(id);
        if (proveedor == null) return NotFound(new { message = "Proveedor no encontrado." });
        return Ok(proveedor);
    }

    // POST api/proveedor
    [Authorize(Roles = "ADMIN")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ProveedorCreateUpdateDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            var created = await _proveedorService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.IdProveedor }, created);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    // PUT api/proveedor/5
    [Authorize(Roles = "ADMIN")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] ProveedorCreateUpdateDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            var updated = await _proveedorService.UpdateAsync(id, dto);
            return Ok(updated);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    // PATCH api/proveedor/5/toggle
    [Authorize(Roles = "ADMIN")]
    [HttpPatch("{id}/toggle")]
    public async Task<IActionResult> Toggle(int id)
    {
        var result = await _proveedorService.ToggleActivoAsync(id);
        if (!result) return NotFound(new { message = "Proveedor no encontrado." });
        return Ok(new { message = "Estado del proveedor cambiado exitosamente." });
    }

    // DELETE api/proveedor/5
    [Authorize(Roles = "ADMIN")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _proveedorService.DeleteAsync(id);
        if (!result) return NotFound(new { message = "Proveedor no encontrado." });
        return Ok(new { message = "Proveedor desactivado exitosamente." });
    }
}
