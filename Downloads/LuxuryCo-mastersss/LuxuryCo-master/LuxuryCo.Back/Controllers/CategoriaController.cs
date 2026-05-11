using LuxuryCo.Database.Data;
using LuxuryCo.Database.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LuxuryCo.Back.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CategoriaController : ControllerBase
{
    private readonly LuxuryCoDbContext _context;

    public CategoriaController(LuxuryCoDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var categorias = await _context.Categorias.OrderBy(c => c.nombre).ToListAsync();
        return Ok(categorias);
    }

    [HttpPost]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> Create([FromBody] CategoriaCreateDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Nombre))
            return BadRequest(new { message = "El nombre de la categoría es requerido." });

        // Evitar duplicados
        var exists = await _context.Categorias.AnyAsync(c => c.nombre.ToLower() == dto.Nombre.ToLower());
        if (exists)
            return Conflict(new { message = "Ya existe una categoría con ese nombre." });

        var categoria = new Categoria
        {
            nombre = dto.Nombre.Trim(),
            descripcion = dto.Descripcion?.Trim()
        };

        _context.Categorias.Add(categoria);
        await _context.SaveChangesAsync();
        return Ok(new { id = categoria.id_categoria, nombre = categoria.nombre });
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> Delete(int id)
    {
        var categoria = await _context.Categorias.FindAsync(id);
        if (categoria == null) return NotFound(new { message = "Categoría no encontrada." });

        // Verificar que no tenga productos asignados
        var tieneProductos = await _context.Productos.AnyAsync(p => p.id_categoria == id);
        if (tieneProductos)
            return Conflict(new { message = "No se puede eliminar esta categoría porque tiene productos asignados." });

        _context.Categorias.Remove(categoria);
        await _context.SaveChangesAsync();
        return Ok(new { message = "Categoría eliminada correctamente." });
    }
}

public record CategoriaCreateDto(string Nombre, string? Descripcion);

