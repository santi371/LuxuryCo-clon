using LuxuryCo.Front.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace LuxuryCo.Front.Controllers
{
    public class ShopController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiBaseUrl = "https://localhost:7066/api";

        public ShopController()
        {
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
            };
            _httpClient = new HttpClient(handler);
        }

        // GET: /Shop/Category/Hombre  or /Shop/Category/Todos
        public async Task<IActionResult> Category(string id)
        {
            var categoryName = id ?? "Hombre";
            ViewData["CategoryName"] = categoryName.ToUpper() == "TODOS" 
                ? "TODA LA COLECCIÓN" 
                : categoryName.ToUpper();

            try
            {
                // Si es "Todos", traemos todos los productos activos
                var endpoint = categoryName.ToLower() == "todos"
                    ? $"{_apiBaseUrl}/producto"
                    : $"{_apiBaseUrl}/producto/category/{categoryName}";

                var response = await _httpClient.GetAsync(endpoint);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var productos = JsonSerializer.Deserialize<List<ProductoViewModel>>(content, options)
                                   ?? new List<ProductoViewModel>();
                    return View(productos);
                }
                else
                {
                    ViewBag.ErrorMessage = "Error al cargar los productos de esta sección.";
                }
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "No pudimos cargar los productos: " + ex.Message;
            }

            return View(new List<ProductoViewModel>());
        }

        // GET: /Shop/Search?q=termino
        public async Task<IActionResult> Search(string q)
        {
            ViewData["CategoryName"] = $"Resultados para \"{q}\"";

            if (string.IsNullOrWhiteSpace(q))
            {
                return View("Category", new List<ProductoViewModel>());
            }

            try
            {
                var response = await _httpClient.GetAsync($"{_apiBaseUrl}/producto/search?q={Uri.EscapeDataString(q)}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var productos = JsonSerializer.Deserialize<List<ProductoViewModel>>(content, options)
                                   ?? new List<ProductoViewModel>();
                    return View("Category", productos);
                }
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "No pudimos realizar la búsqueda: " + ex.Message;
            }

            return View("Category", new List<ProductoViewModel>());
        }

        // GET: /Shop/Product/1
        public async Task<IActionResult> Product(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_apiBaseUrl}/producto/{id}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var producto = JsonSerializer.Deserialize<ProductoViewModel>(content, options);
                    return View(producto);
                }
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "No se pudo cargar el producto: " + ex.Message;
            }

            return RedirectToAction("Index", "Home");
        }
    }
}

