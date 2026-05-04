using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using LuxuryCo.Front.ViewModels;

namespace LuxuryCo.Front.Controllers;

[Authorize]
public class CartController : Controller
{
    private readonly HttpClient _httpClient;
    private readonly string _apiBaseUrl = "https://localhost:7066/api/carrito";

    public CartController()
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
        try
        {
            var response = await _httpClient.GetAsync(_apiBaseUrl);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var cart = JsonSerializer.Deserialize<CarritoViewModel>(content, options);
                return View(cart);
            }
        }
        catch (Exception ex)
        {
            ViewBag.ErrorMessage = "Error al cargar el carrito: " + ex.Message;
        }

        return View(new CarritoViewModel());
    }

    [HttpPost]
    public async Task<IActionResult> Add(int productId, int quantity = 1)
    {
        AddAuthorizationHeader();
        try
        {
            var requestBody = new { IdProducto = productId, Cantidad = quantity };
            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{_apiBaseUrl}/add", content);
            
            var responseContent = await response.Content.ReadAsStringAsync();
            var resultDict = JsonSerializer.Deserialize<Dictionary<string, string>>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (response.IsSuccessStatusCode)
            {
                TempData["CartMessage"] = resultDict?.GetValueOrDefault("message", "Producto añadido al carrito.");
                return RedirectToAction("Index");
            }
            else
            {
                TempData["CartError"] = resultDict?.GetValueOrDefault("message", "Error al añadir producto.");
            }
        }
        catch (Exception ex)
        {
            TempData["CartError"] = "Excepción al añadir producto: " + ex.Message;
        }

        return RedirectToAction("Product", "Shop", new { id = productId });
    }

    [HttpPost]
    public async Task<IActionResult> Remove(int id)
    {
        AddAuthorizationHeader();
        try
        {
            var response = await _httpClient.DeleteAsync($"{_apiBaseUrl}/remove/{id}");
            if (response.IsSuccessStatusCode)
            {
                TempData["CartMessage"] = "Producto eliminado.";
            }
        }
        catch (Exception ex)
        {
            TempData["CartError"] = "Error: " + ex.Message;
        }
        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> Update(int id, int quantity)
    {
        AddAuthorizationHeader();
        try
        {
            var json = JsonSerializer.Serialize(quantity);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync($"{_apiBaseUrl}/update/{id}", content);
            
            var responseContent = await response.Content.ReadAsStringAsync();
            var resultDict = string.IsNullOrWhiteSpace(responseContent) ? null : JsonSerializer.Deserialize<Dictionary<string, string>>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (response.IsSuccessStatusCode)
            {
                TempData["CartMessage"] = resultDict?.GetValueOrDefault("message", "Cantidad actualizada.");
            }
            else
            {
                TempData["CartError"] = resultDict?.GetValueOrDefault("message", "Error al actualizar cantidad.");
            }
        }
        catch (Exception ex)
        {
            TempData["CartError"] = "Excepción: " + ex.Message;
        }
        return RedirectToAction("Index");
    }
}
