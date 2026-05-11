using LuxuryCo.Database.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LuxuryCo.Back.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "ADMIN")]
    public class SearchController : ControllerBase
    {
        private readonly LuxuryCoDbContext _context;

        public SearchController(LuxuryCoDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Search([FromQuery] string q, [FromQuery] string filter = "all")
        {
            if (string.IsNullOrWhiteSpace(q) || q.Length < 2)
                return Ok(new List<object>());

            var query = q.ToLower();
            var results = new List<object>();

            if (filter == "all" || filter == "producto")
            {
                var productos = await _context.Productos
                    .Where(p => p.nombre.ToLower().Contains(query) || (p.descripcion != null && p.descripcion.ToLower().Contains(query)))
                    .Take(5)
                    .Select(p => new { type = "producto", id = p.id_producto, name = p.nombre, detail = $"${p.precio}" })
                    .ToListAsync();
                results.AddRange(productos);
            }

            if (filter == "all" || filter == "marca")
            {
                var marcas = await _context.Marcas
                    .Where(m => m.nombre.ToLower().Contains(query))
                    .Take(5)
                    .Select(m => new { type = "marca", id = m.id_marca, name = m.nombre, detail = "Marca" })
                    .ToListAsync();
                results.AddRange(marcas);
            }

            if (filter == "all" || filter == "usuario")
            {
                var usuarios = await _context.Usuarios
                    .Where(u => u.nombre.ToLower().Contains(query) || 
                                (u.apellido != null && u.apellido.ToLower().Contains(query)) || 
                                u.email.ToLower().Contains(query))
                    .Take(5)
                    .Select(u => new { type = "usuario", id = u.id_usuario, name = $"{u.nombre} {u.apellido}".Trim(), detail = u.email })
                    .ToListAsync();
                results.AddRange(usuarios);
            }

            return Ok(results);
        }
    }
}
