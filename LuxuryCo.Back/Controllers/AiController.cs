using LuxuryCo.Back.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LuxuryCo.Back.Controllers;

// Controlador encargado de exponer los endpoints de Inteligencia Artificial para el Frontend
[ApiController]
[Route("api/[controller]")]
public class AiController : ControllerBase
{
    private readonly IAiService _aiService;

    // Inyección de dependencias: se inyecta el servicio que contiene la lógica de Groq
    public AiController(IAiService aiService)
    {
        _aiService = aiService;
    }

    // Endpoint seguro para el chat del Administrador (solo accesible con token JWT válido de Rol ADMIN)
    // Ruta: POST /api/Ai/admin-chat
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
            // Llama al servicio de IA configurado para análisis de negocio
            var response = await _aiService.GetAdminBusinessAdviceAsync(request.Message);
            return Ok(new { reply = response });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error interno de la IA", details = ex.Message });
        }
    }

    // Endpoint público para el widget flotante del Estilista (accesible por cualquier cliente)
    // Ruta: POST /api/Ai/stylist-chat
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
            // Llama al servicio de IA configurado como Asesor de Estilo, pasando el SessionId para mantener la memoria
            var result = await _aiService.GetClientStylistAdviceAsync(request.Message, request.SessionId ?? "default");
            
            // Retorna tanto el texto de respuesta como las tarjetas de productos recomendados
            return Ok(new { reply = result.Reply, cards = result.Cards });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error interno de la IA", details = ex.Message });
        }
    }
}

// DTO (Objeto de Transferencia de Datos) para las peticiones del Administrador
public class AiRequest
{
    // Mensaje escrito por el administrador
    public string Message { get; set; } = string.Empty;
}

// DTO para las peticiones del widget del cliente (incluye manejo de sesión)
public class StylistAiRequest
{
    // Mensaje escrito por el cliente
    public string Message { get; set; } = string.Empty;
    
    // Identificador único generado por el navegador para recordar el historial del chat
    public string SessionId { get; set; } = string.Empty;
}
