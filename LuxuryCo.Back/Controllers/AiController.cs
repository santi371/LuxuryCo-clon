using LuxuryCo.Back.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LuxuryCo.Back.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AiController : ControllerBase
{
    private readonly IAiService _aiService;

    public AiController(IAiService aiService)
    {
        _aiService = aiService;
    }

    [HttpPost("admin-chat")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> AdminChat([FromBody] AiRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Message))
        {
            return BadRequest(new { message = "El mensaje no puede estar vacío." });
        }

        try
        {
            var response = await _aiService.GetAdminBusinessAdviceAsync(request.Message);
            return Ok(new { reply = response });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error interno de la IA", details = ex.Message });
        }
    }
    [HttpPost("stylist-chat")]
    [AllowAnonymous]
    public async Task<IActionResult> StylistChat([FromBody] StylistAiRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Message))
        {
            return BadRequest(new { message = "El mensaje no puede estar vacío." });
        }

        try
        {
            var response = await _aiService.GetClientStylistAdviceAsync(request.Message, request.SessionId ?? "default");
            return Ok(new { reply = response });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error interno de la IA", details = ex.Message });
        }
    }
}

public class AiRequest
{
    public string Message { get; set; } = string.Empty;
}

public class StylistAiRequest
{
    public string Message { get; set; } = string.Empty;
    public string SessionId { get; set; } = string.Empty;
}
