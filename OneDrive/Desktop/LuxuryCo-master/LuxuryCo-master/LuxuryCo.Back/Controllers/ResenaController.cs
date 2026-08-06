using LuxuryCo.Database.Data;
using LuxuryCo.Database.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LuxuryCo.Back.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ResenaController : ControllerBase
{
    private readonly LuxuryCoDbContext _context;

    public ResenaController(LuxuryCoDbContext context)
    {
        _context = context;
    }

    // GET api/resena/producto/101
    [HttpGet("producto/{idProducto}")]
    public async Task<IActionResult> GetByProducto(int idProducto)
    {
        var resenas = await _context.Resenas
            .Where(r => r.id_producto == idProducto)
            .Include(r => r.Usuario)
            .OrderByDescending(r => r.fecha)
            .Select(r => new
            {
                r.id_resena,
                r.calificacion,
                r.comentario,
                fecha = r.fecha.ToString("yyyy-MM-dd"),
                nombreUsuario = r.Usuario != null ? r.Usuario.nombre : (!string.IsNullOrEmpty(r.nombre_invitado) ? r.nombre_invitado : "Anónimo"),
                avatarUrl = (string?)null
            })
            .ToListAsync();

        var promedio = resenas.Any() ? resenas.Average(r => r.calificacion) : 0;

        return Ok(new { resenas, promedio = Math.Round(promedio, 1), total = resenas.Count });
    }

    // POST api/resena
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ResenaCreateDto dto)
    {
        if (dto.Calificacion < 1 || dto.Calificacion > 5)
            return BadRequest(new { message = "La calificación debe estar entre 1 y 5." });

        if (string.IsNullOrWhiteSpace(dto.Comentario))
            return BadRequest(new { message = "El comentario no puede estar vacío." });

        // Verificar que el producto existe
        var producto = await _context.Productos.FindAsync(dto.IdProducto);
        if (producto == null)
            return NotFound(new { message = "Producto no encontrado." });

        var resena = new Resena
        {
            id_producto  = dto.IdProducto,
            id_usuario   = dto.IdUsuario > 0 ? dto.IdUsuario : null,
            calificacion = dto.Calificacion,
            comentario   = dto.Comentario.Trim(),
            nombre_invitado = dto.IdUsuario == 0 ? dto.NombreInvitado.Trim() : null,
            fecha        = DateTime.UtcNow
        };

        _context.Resenas.Add(resena);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Reseña publicada. ¡Gracias por tu opinión!", id = resena.id_resena });
    }
}

// DTO para crear reseña (sin archivo DTO separado para mantenerlo simple)
public class ResenaCreateDto
{
    public int    IdProducto   { get; set; }
    public int    IdUsuario    { get; set; }
    public int    Calificacion { get; set; }
    public string Comentario   { get; set; } = string.Empty;
    public string NombreInvitado { get; set; } = string.Empty;
}
