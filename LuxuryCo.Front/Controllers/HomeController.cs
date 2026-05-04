using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using LuxuryCo.Front.Models;

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

    [HttpPost]
    public async Task<IActionResult> SendStylistMessage([FromBody] object request)
    {
        // Enviamos la petición anónima al backend AI Controller
        var jsonContent = new StringContent(System.Text.Json.JsonSerializer.Serialize(request), System.Text.Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync($"{_apiBaseUrl}/Ai/stylist-chat", jsonContent);

        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            return Content(content, "application/json");
        }
        
        return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());
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
