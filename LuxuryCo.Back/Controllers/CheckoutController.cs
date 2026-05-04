using System.Security.Claims;
using System.Threading.Tasks;
using LuxuryCo.Back.DTOs;
using LuxuryCo.Back.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LuxuryCo.Back.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class CheckoutController : ControllerBase
{
    private readonly ICheckoutService _checkoutService;

    public CheckoutController(ICheckoutService checkoutService)
    {
        _checkoutService = checkoutService;
    }

    private int GetUserId()
    {
        var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.Parse(idClaim ?? "0");
    }

    [HttpPost("process")]
    public async Task<IActionResult> ProcessCheckout([FromBody] CheckoutRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new { Success = false, Message = "Datos de envío inválidos." });
        }

        var idUsuario = GetUserId();
        
        var result = await _checkoutService.PlaceOrderAsync(idUsuario, request);

        if (result.Success)
        {
            return Ok(result);
        }
        else
        {
            return BadRequest(result);
        }
    }
}
