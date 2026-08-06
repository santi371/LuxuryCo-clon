using LuxuryCo.Database.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LuxuryCo.Back.Controllers;

[Route("api/[controller]")]
[ApiController]
public class MarcaController : ControllerBase
{
    private readonly LuxuryCoDbContext _context;

    public MarcaController(LuxuryCoDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var marcas = await _context.Marcas.ToListAsync();
        return Ok(marcas);
    }
}
