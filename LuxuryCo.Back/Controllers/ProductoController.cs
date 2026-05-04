using LuxuryCo.Back.DTOs;
using LuxuryCo.Back.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LuxuryCo.Back.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ProductoController : ControllerBase
{
    private readonly IProductoService _productoService;

    public ProductoController(IProductoService productoService)
    {
        _productoService = productoService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        // Si el usuario es admin y está autenticado, podría ver también los inactivos
        bool isAdmin = User.Identity.IsAuthenticated && User.IsInRole("ADMIN");
        var productos = await _productoService.GetAllAsync(isAdmin);
        return Ok(productos);
    }

    [HttpGet("category/{categoryName}")]
    public async Task<IActionResult> GetByCategory(string categoryName)
    {
        bool isAdmin = User.Identity.IsAuthenticated && User.IsInRole("ADMIN");
        var productos = await _productoService.GetByCategoryAsync(categoryName, isAdmin);
        
        // Return OK even if empty so the frontend can just render an empty grid
        return Ok(productos);
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string q)
    {
        if (string.IsNullOrWhiteSpace(q)) return Ok(new List<ProductoDto>());
        
        bool isAdmin = User.Identity.IsAuthenticated && User.IsInRole("ADMIN");
        var productos = await _productoService.SearchAsync(q, isAdmin);
        
        return Ok(productos);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var producto = await _productoService.GetByIdAsync(id);
            return Ok(producto);
        }
        catch (Exception ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    // [Authorize(Roles = "ADMIN")] // Protegido por el AdminController en el frontend
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ProductoCreateUpdateDto dto)
    {
        try
        {
            var producto = await _productoService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = producto.IdProducto }, producto);
        }
        catch (Exception ex)
        {
            var inner = ex.InnerException?.Message ?? ex.Message;
            return BadRequest(new { message = inner });
        }
    }

    // [Authorize(Roles = "ADMIN")] // Protegido por el AdminController en el frontend
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] ProductoCreateUpdateDto dto)
    {
        try
        {
            var producto = await _productoService.UpdateAsync(id, dto);
            return Ok(producto);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // [Authorize(Roles = "ADMIN")] // Protegido por el AdminController en el frontend
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _productoService.DeleteAsync(id);
            return Ok(new { message = "Producto eliminado correctamente." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = "No se pudo eliminar (probablemente tenga ventas asociadas): " + ex.Message });
        }
    }

    [HttpPut("{id}/toggle-status")]
    public async Task<IActionResult> ToggleStatus(int id)
    {
        try
        {
            await _productoService.ToggleStateAsync(id);
            return Ok(new { message = "Estado del producto cambiado correctamente." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id}/imagenes")]
    public async Task<IActionResult> UploadImages(int id, List<IFormFile> imagenes)
    {
        try
        {
            var result = await _productoService.UploadImagesAsync(id, imagenes);
            if (result) return Ok(new { message = "Imágenes subidas correctamente." });
            return BadRequest(new { message = "No se procesaron imágenes (lista vacía)." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
    [HttpDelete("{id}/imagen/{idImagen}")]
    public async Task<IActionResult> DeleteImage(int id, int idImagen)
    {
        try
        {
            await _productoService.DeleteImageAsync(id, idImagen);
            return Ok(new { message = "Imagen eliminada correctamente." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id}/imagen/{idImagen}/principal")]
    public async Task<IActionResult> SetPrincipalImage(int id, int idImagen)
    {
        try
        {
            await _productoService.SetPrincipalImageAsync(id, idImagen);
            return Ok(new { message = "Imagen establecida como principal." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
