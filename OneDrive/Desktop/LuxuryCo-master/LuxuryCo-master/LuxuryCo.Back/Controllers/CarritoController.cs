using LuxuryCo.Back.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LuxuryCo.Back.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize] // Solo usuarios logueados pueden tener carrito persistente
public class CarritoController : ControllerBase
{
    private readonly ICarritoService _carritoService;

    public CarritoController(ICarritoService carritoService)
    {
        _carritoService = carritoService;
    }

    private int GetUserId()
    {
        var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.Parse(idClaim ?? "0");
    }

    [HttpGet]
    public async Task<IActionResult> GetCart()
    {
        var idUsuario = GetUserId();
        var carritoDto = await _carritoService.GetOrCreateCartAsync(idUsuario);
        return Ok(carritoDto);
    }

    [HttpPost("add")]
    public async Task<IActionResult> AddToCart([FromBody] AddToCartRequest request)
    {
        var idUsuario = GetUserId();
        try 
        {
            var success = await _carritoService.AddToCartAsync(idUsuario, request.IdProducto, request.Cantidad, request.Talla);
            if (success) return Ok(new { message = "Producto añadido al carrito." });
            return BadRequest(new { message = "No se pudo añadir el producto." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("update/{id}")]
    public async Task<IActionResult> UpdateQuantity(int id, [FromBody] int cantidad)
    {
        var idUsuario = GetUserId();
        try 
        {
            var success = await _carritoService.UpdateQuantityAsync(idUsuario, id, cantidad);
            if (success) return Ok(new { message = "Cantidad actualizada." });
            return BadRequest(new { message = "Error al actualizar cantidad." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("remove/{id}")]
    public async Task<IActionResult> Remove(int id)
    {
        var idUsuario = GetUserId();
        var success = await _carritoService.RemoveFromCartAsync(idUsuario, id);
        if (success) return Ok(new { message = "Producto eliminado del carrito." });
        return BadRequest(new { message = "Error al eliminar producto." });
    }

    [HttpDelete("clear")]
    public async Task<IActionResult> Clear()
    {
        var idUsuario = GetUserId();
        await _carritoService.ClearCartAsync(idUsuario);
        return Ok(new { message = "Carrito vaciado." });
    }
}

public class AddToCartRequest
{
    public int IdProducto { get; set; }
    public int Cantidad { get; set; }
    public string? Talla { get; set; }
}
