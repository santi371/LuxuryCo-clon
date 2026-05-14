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
    private readonly string _apiBaseUrl = "https://localhost:7066/api";

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
    // Envía la petición aquí, y este método se encarga de enviarla al Backend de forma segura.
    [HttpPost]
    public async Task<IActionResult> SendStylistMessage([FromBody] StylistProxyRequest request)
    {
        try
        {
            // 1. Preparar los datos que se enviarán al Backend
            // Se usa un objeto fuertemente tipado para evitar errores de parseo de JSON en el Backend.
            var payload = new { message = request.Message, sessionId = request.SessionId };
            var jsonContent = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            
            // 2. Hacer la petición al Backend real (puerto 7066)
            var response = await _httpClient.PostAsync($"{_apiBaseUrl}/Ai/stylist-chat", jsonContent);

            var rawContent = await response.Content.ReadAsStringAsync();

            // 3. Si todo salió bien, reenviar la respuesta de la IA al navegador del usuario
            if (response.IsSuccessStatusCode)
            {
                return Content(rawContent, "application/json");
            }

            // 4. Manejo de Errores: Siempre devolver JSON válido al Frontend
            // Esto evita el error de JavaScript "Unexpected end of JSON input" cuando el servidor falla.
            return StatusCode(
                (int)response.StatusCode,
                JsonSerializer.Serialize(new { message = $"Error del servidor ({(int)response.StatusCode})", details = rawContent })
            );
        }
        catch (Exception ex)
        {
            // Si el Backend está apagado o no responde, evitamos que la página explote
            // y devolvemos un mensaje en formato JSON que el widget pueda entender y mostrar.
            Response.ContentType = "application/json";
            return StatusCode(503, JsonSerializer.Serialize(new { message = "El servidor de IA no está disponible. ¿Está corriendo el Backend?", details = ex.Message }));
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

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}

public class StylistProxyRequest
{
    public string Message { get; set; } = string.Empty;
    public string SessionId { get; set; } = string.Empty;
}
