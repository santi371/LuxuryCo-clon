using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using LuxuryCo.Front.Models;
using System.Text;
using System.Text.Json;

namespace LuxuryCo.Front.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly HttpClient _httpClient;
    private readonly string _apiBaseUrl = (Environment.GetEnvironmentVariable("API_BASE_URL") ?? "http://localhost:5137") + "/api";

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
        var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
        };
        _httpClient = new HttpClient(handler);
    }

    // Endpoint "Proxy" (Puente) para el chat del Estilista
    // El navegador (Frontend) por seguridad no hace peticiones directas al Backend.
    // Envía la peticin aquí, y este método se encarga de enviarla al Backend de forma segura.
    [HttpPost]
    public async Task<IActionResult> SendStylistMessage([FromBody] StylistProxyRequest request)
    {
        try
        {
            // 1. Preparar los datos que se enviarán al Backend
            var payload = new
            {
                message = request.Message,
                sessionId = request.SessionId,
                userId = request.UserId,
                history = request.History,
                lastProductId = request.LastProductId
            };
            var jsonContent = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            
            // 2. Hacer la petición al Backend real
            var response = await _httpClient.PostAsync($"{_apiBaseUrl}/Ai/stylist-chat", jsonContent);

            var rawContent = await response.Content.ReadAsStringAsync();

            // 3. Si todo salió bien, reenviar la respuesta de la IA al navegador del usuario
            if (response.IsSuccessStatusCode)
            {
                return Content(rawContent, "application/json");
            }

            // 4. Manejo de Errores: Siempre devolver JSON válido al Frontend
            return StatusCode(
                (int)response.StatusCode,
                JsonSerializer.Serialize(new { message = $"Error del servidor ({(int)response.StatusCode})", details = rawContent })
            );
        }
        catch (Exception ex)
        {
            Response.ContentType = "application/json";
            return StatusCode(503, JsonSerializer.Serialize(new { message = "El servidor de IA no está disponible. ¿Está corriendo el Backend?", details = ex.Message }));
        }
    }

    [HttpPost]
    public async Task<IActionResult> TranscribeAudio(IFormFile file)
    {
        try
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { message = "Archivo vacío" });

            using var content = new MultipartFormDataContent();
            using var stream = file.OpenReadStream();
            using var streamContent = new StreamContent(stream);
            content.Add(streamContent, "file", file.FileName);

            var response = await _httpClient.PostAsync($"{_apiBaseUrl}/Ai/transcribe", content);
            var rawContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                return Content(rawContent, "application/json");
            }
            return StatusCode((int)response.StatusCode, rawContent);
        }
        catch (Exception ex)
        {
            return StatusCode(503, JsonSerializer.Serialize(new { message = "Error de proxy transcribe", details = ex.Message }));
        }
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }
    
    public IActionResult Nosotros()
    {
        return View();
    }

    public IActionResult Refunds()
    {
        return View();
    }

    public IActionResult Shipping()
    {
        return View();
    }

    public IActionResult Distributors()
    {
        return View();
    }

    public IActionResult B2B()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> VirtualTryOn([FromBody] VirtualTryOnProxyRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.UserPhotoBase64))
                return BadRequest(new { message = "La foto es requerida." });

            var payload = new
            {
                userPhotoBase64   = request.UserPhotoBase64,
                userPhotoMimeType = request.UserPhotoMimeType,
                productId         = request.ProductId,
                garmentDescription = request.GarmentDescription,
                garmentImageUrl   = request.GarmentImageUrl,
                category          = request.Category,
                seed              = request.Seed
            };
            var jsonContent = new StringContent(JsonSerializer.Serialize(payload), System.Text.Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{_apiBaseUrl}/Ai/virtual-tryon", jsonContent);
            var rawContent = await response.Content.ReadAsStringAsync();
            return Content(rawContent, "application/json");
        }
        catch (Exception ex)
        {
            return StatusCode(503, JsonSerializer.Serialize(new { message = "Error de proxy del probador virtual.", details = ex.Message }));
        }
    }

    [HttpPost]
    public async Task<IActionResult> GenerateImageProxy([FromBody] ImageGenProxyRequest request)
    {
        try
        {
            var payload = new
            {
                prompt = request.Prompt,
                userId = request.UserId,
                seed = request.Seed
            };
            var jsonContent = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{_apiBaseUrl}/Ai/generate-image", jsonContent);
            var rawContent = await response.Content.ReadAsStringAsync();

            return Content(rawContent, "application/json");
        }
        catch (Exception ex)
        {
            return StatusCode(503, JsonSerializer.Serialize(new { message = "Error de proxy de imagen", details = ex.Message }));
        }
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}

public class ImageGenProxyRequest
{
    public string Prompt { get; set; } = string.Empty;
    public int? UserId { get; set; }
    public int? Seed { get; set; }
}

public class StylistProxyRequest
{
    public string Message { get; set; } = string.Empty;
    public string SessionId { get; set; } = string.Empty;
    public int? UserId { get; set; }
    public List<ChatHistoryEntryFront> History { get; set; } = new();
    public int? LastProductId { get; set; }
}

public class ChatHistoryEntryFront
{
    public string Role    { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}

public class VirtualTryOnProxyRequest
{
    public string  UserPhotoBase64    { get; set; } = string.Empty;
    public string? UserPhotoMimeType  { get; set; }
    public int?    ProductId          { get; set; }
    public string? GarmentDescription { get; set; }
    public string? GarmentImageUrl    { get; set; }
    public string? Category           { get; set; }
    public int?    Seed               { get; set; }
}

