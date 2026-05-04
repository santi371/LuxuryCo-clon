using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using LuxuryCo.Front.ViewModels;

namespace LuxuryCo.Front.Controllers;

[Authorize]
public class CheckoutController : Controller
{
    private readonly HttpClient _httpClient;
    private readonly string _apiCheckoutUrl = "https://localhost:7066/api/checkout/process";
    private readonly string _apiCartUrl = "https://localhost:7066/api/carrito";

    public CheckoutController()
    {
        var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
        };
        _httpClient = new HttpClient(handler);
    }

    private void AddAuthorizationHeader()
    {
        var token = HttpContext.Request.Cookies["jwt_token"] ?? HttpContext.Session.GetString("JWT_Token");
        if (!string.IsNullOrEmpty(token))
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        AddAuthorizationHeader();
        
        // Cargar el Resumen del Carrito para la Vista de Checkout
        var response = await _httpClient.GetAsync(_apiCartUrl);
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var cart = JsonSerializer.Deserialize<CarritoViewModel>(content, options);
            
            if (cart == null || !cart.Detalles.Any())
            {
                TempData["CartError"] = "Tu carrito está vacío. Agrega productos para proceder al pago.";
                return RedirectToAction("Index", "Cart");
            }

            ViewBag.CartSummary = cart;
        }

        return View(new CheckoutRequestViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PlaceOrder(CheckoutRequestViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return RedirectToAction("Index"); // La validación rebotará
        }

        AddAuthorizationHeader();

        try
        {
            var json = JsonSerializer.Serialize(model);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(_apiCheckoutUrl, content);
            var responseContent = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            if (response.IsSuccessStatusCode)
            {
                var result = JsonSerializer.Deserialize<CheckoutResponseViewModel>(responseContent, options);
                TempData["OrderId"] = result?.OrderId?.ToString();
                TempData["TrackingNumber"] = result?.TrackingNumber;
                return RedirectToAction("Success");
            }
            else
            {
                // Extraer el mensaje de error Custom del Backend (Ej. Stock agotado)
                var errorResult = JsonSerializer.Deserialize<CheckoutResponseViewModel>(responseContent, options);
                TempData["CheckoutError"] = errorResult?.Message ?? "Un error imprevisto ocurrió procesando tu tarjeta.";
                return RedirectToAction("Index");
            }
        }
        catch (Exception ex)
        {
            TempData["CheckoutError"] = "Error de red conectando con la pasarela: " + ex.Message;
            return RedirectToAction("Index");
        }
    }

    [HttpGet]
    public IActionResult Success()
    {
        if (TempData["OrderId"] == null)
        {
            return RedirectToAction("Index", "Home");
        }
        
        return View();
    }
}
