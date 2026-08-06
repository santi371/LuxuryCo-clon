using LuxuryCo.Back.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text.Json;

namespace LuxuryCo.Back.Controllers;

// Controlador encargado de exponer los endpoints de Inteligencia Artificial para el Frontend
[ApiController]
[Route("api/[controller]")]
public class AiController : ControllerBase
{
    private readonly IAiService _aiService;
    private readonly WhisperProvider _whisperProvider;
    private readonly ConfirmationService _confirmationService;
    private readonly ToolExecutorService _toolExecutor;
    private readonly ImageGenerationService _imageGenService;
    private readonly VirtualTryOnService _virtualTryOnService;
    private readonly LuxuryCo.Database.Data.LuxuryCoDbContext _context;

    public AiController(
        IAiService aiService, 
        WhisperProvider whisperProvider, 
        ConfirmationService confirmationService,
        ToolExecutorService toolExecutor,
        ImageGenerationService imageGenService,
        VirtualTryOnService virtualTryOnService,
        LuxuryCo.Database.Data.LuxuryCoDbContext context)
    {
        _aiService = aiService;
        _whisperProvider = whisperProvider;
        _confirmationService = confirmationService;
        _toolExecutor = toolExecutor;
        _imageGenService = imageGenService;
        _virtualTryOnService = virtualTryOnService;
        _context = context;
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
            var idClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            int adminUserId = int.TryParse(idClaim, out int id) ? id : 0;

            // Llama al servicio de IA configurado para análisis de negocio
            var response = await _aiService.GetAdminBusinessAdviceAsync(request.Message, adminUserId);
            return Ok(new { reply = response });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error interno de la IA", details = "Por favor, intenta de nuevo más tarde." });
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
            // Llama al servicio de IA configurado como Asesor de Estilo
            var result = await _aiService.GetClientStylistAdviceAsync(
                request.Message,
                request.SessionId ?? "default",
                request.UserId,
                request.History,
                request.LastProductId);
            
            // Retorna tanto el texto de respuesta como las tarjetas de productos recomendados
            return Ok(new { reply = result.Reply, cards = result.Cards });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error interno de la IA", details = "Por favor, intenta de nuevo más tarde." });
        }
    }

    // Endpoint para transcribir audio usando Whisper
    // Ruta: POST /api/Ai/transcribe
    [HttpPost("transcribe")]
    [AllowAnonymous]
    public async Task<IActionResult> TranscribeAudio(Microsoft.AspNetCore.Http.IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { message = "El archivo de audio no puede estar vacío." });
        }

        try
        {
            using var ms = new MemoryStream();
            await file.CopyToAsync(ms);
            var audioBytes = ms.ToArray();

            var text = await _whisperProvider.TranscribeAudioAsync(audioBytes, file.FileName);
            return Ok(new { text });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al transcribir el audio", details = "Por favor, intenta de nuevo más tarde." });
        }
    }

    // Endpoint para aprobar una acción pendiente por confirmación del administrador
    // Ruta: POST /api/Ai/approve-action
    [HttpPost("approve-action")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> ApproveAction([FromBody] ActionConfirmRequest request)
    {
        if (string.IsNullOrEmpty(request.Token))
        {
            return BadRequest(new { message = "El token de acción es requerido." });
        }

        try
        {
            var pending = await _confirmationService.GetPendingActionAsync(request.Token);
            if (pending == null)
            {
                return NotFound(new { message = "Acción pendiente caducada o no encontrada." });
            }

            var idClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            int adminUserId = int.TryParse(idClaim, out int id) ? id : 0;

            if (pending.UserId != adminUserId)
            {
                return Forbid("No tienes autorización sobre esta acción pendiente.");
            }

            // Execute using the tool executor
            var paramsObj = JsonSerializer.Deserialize<IntentParameters>(JsonSerializer.Serialize(pending.Parameters));
            var toolResult = await _toolExecutor.ExecuteToolAsync(pending.Intent, paramsObj ?? new IntentParameters(), adminUserId);

            await _confirmationService.CompleteActionAsync(request.Token);

            return Ok(new { success = toolResult.Success, message = toolResult.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al procesar la aprobación", details = "Por favor, intenta de nuevo más tarde." });
        }
    }

    [HttpPost("generate-image")]
    [AllowAnonymous]
    public async Task<IActionResult> GenerateImage([FromBody] ImageGenRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Prompt))
        {
            return BadRequest(new { message = "El prompt no puede estar vacío." });
        }

        try
        {
            var result = await _imageGenService.GenerateImageAsync(request.Prompt, request.UserId, request.Seed);
            if (result.Status == "Failed")
            {
                return StatusCode(500, result);
            }
            if (result.Status == "Blocked" || result.Status == "QuotaExceeded" || result.Status == "Cooldown")
            {
                return BadRequest(result);
            }
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al generar imagen de lujo.", details = "Por favor, intenta de nuevo más tarde." });
        }
    }

    // Endpoint del Probador Virtual con IA
    // Ruta: POST /api/Ai/virtual-tryon
    [HttpPost("virtual-tryon")]
    [AllowAnonymous]
    public async Task<IActionResult> VirtualTryOn([FromBody] VirtualTryOnRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.UserPhotoBase64))
            return BadRequest(new { message = "La foto del usuario es requerida." });

        try
        {
            // Si viene productId, buscar la imagen y descripcion del producto
            string garmentDescription = request.GarmentDescription ?? "luxury fashion garment";
            string? garmentImageUrl = request.GarmentImageUrl;

            if (request.ProductId.HasValue && request.ProductId.Value > 0)
            {
                var product = await _context.Productos
                    .Where(p => p.id_producto == request.ProductId.Value)
                    .Select(p => new
                    {
                        p.nombre,
                        p.descripcion,
                        imagen = p.Imagenes
                            .Where(i => i.principal)
                            .Select(i => i.url_imagen)
                            .FirstOrDefault()
                    })
                    .FirstOrDefaultAsync();

                if (product != null)
                {
                    garmentDescription = $"{product.nombre} - {product.descripcion ?? string.Empty}";
                    garmentImageUrl ??= product.imagen;
                }
            }

            var result = await _virtualTryOnService.TryOnAsync(
                request.UserPhotoBase64,
                request.UserPhotoMimeType ?? "image/jpeg",
                garmentDescription,
                garmentImageUrl,
                request.Category ?? "tops",
                request.Seed ?? 0);

            if (result.Status == "Failed")
                return StatusCode(500, new { message = result.ErrorMessage });

            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error en el probador virtual.", details = "Por favor, intenta de nuevo más tarde." });
        }
    }
    // Endpoint para carga segura de archivos y documentos de contexto
    // Ruta: POST /api/Ai/upload-context-file
    [HttpPost("upload-context-file")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> UploadContextFile(Microsoft.AspNetCore.Http.IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { message = "El archivo no puede estar vacío." });
        }

        try
        {
            // Validaciones de Magic Bytes, MIME Type Real y Anti-Malware (Mock)
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var allowedExtensions = new[] { ".csv", ".txt", ".pdf", ".png", ".jpg" };
            if (!allowedExtensions.Contains(extension))
            {
                return BadRequest(new { message = "Extensión de archivo no permitida por políticas de seguridad." });
            }

            // Simulamos guardado en Storage Seguro y generación de Signed URL
            string secureRef = $"https://storage.luxuryco.internal/secure-sandbox/{Guid.NewGuid()}{extension}?sig=MockSignature";

            return Ok(new { secureReferenceUrl = secureRef });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al procesar el archivo seguro.", details = "Intente nuevamente." });
        }
    }
}

public class ActionConfirmRequest
{
    public string Token { get; set; } = string.Empty;
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

    // ID del usuario autenticado (0 o null = visitante no logueado)
    public int? UserId { get; set; }

    // Historial de la conversación (últimos N turnos) para dar contexto a la IA
    public List<ChatHistoryEntry> History { get; set; } = new();

    // ID del último producto que la IA mostró al usuario en tarjeta (para contexto "agrégalo")
    public int? LastProductId { get; set; }
}

public class ImageGenRequest
{
    public string Prompt { get; set; } = string.Empty;
    public int? UserId { get; set; }
    public int? Seed { get; set; }
}

// DTO para el Probador Virtual con IA
public class VirtualTryOnRequest
{
    public string UserPhotoBase64   { get; set; } = string.Empty;
    public string? UserPhotoMimeType { get; set; }
    public int?   ProductId         { get; set; }
    public string? GarmentDescription { get; set; }
    public string? GarmentImageUrl  { get; set; }
    /// Categoria: tops | bottoms | one-pieces
    public string? Category         { get; set; }
    public int?   Seed              { get; set; }
}
