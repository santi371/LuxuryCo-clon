using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Net.Http.Headers;
using LuxuryCo.Front.ViewModels;
using System.Text;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace LuxuryCo.Front.Controllers;

[Authorize(Roles = "ADMIN")]
public class AdminController : Controller
{
    private readonly HttpClient _httpClient;
    private readonly Microsoft.AspNetCore.Hosting.IWebHostEnvironment _env;
    private readonly string _apiBaseUrl = "https://localhost:7066/api";

    public AdminController(Microsoft.AspNetCore.Hosting.IWebHostEnvironment env)
    {
        _env = env;
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

    // ============================================================
    // DASHBOARD
    // ============================================================

    public IActionResult Index()
    {
        // El dashboard principal será estático (solo el menú) o podría cargar pequeñas estadísticas
        return View();
    }

    public IActionResult AiAssistant()
    {
        return View();
    }

    [HttpPost]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> SendAiMessage([FromBody] Dictionary<string, string> request)
    {
        try
        {
            AddAuthorizationHeader();

            if (!request.TryGetValue("message", out var message) || string.IsNullOrWhiteSpace(message))
            {
                return BadRequest(JsonSerializer.Serialize(new { message = "El mensaje no puede estar vacío." }));
            }

            var payload = new { message = message };
            var jsonContent = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{_apiBaseUrl}/Ai/admin-chat", jsonContent);

            var rawContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                return Content(rawContent, "application/json");
            }

            // 401: El token no está o expiró
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                return StatusCode(401, JsonSerializer.Serialize(new
                {
                    message = "Tu sesión ha expirado.",
                    details = "Cierra sesión y vuelve a iniciarla para continuar."
                }));
            }

            // Cualquier otro error del backend: devolver JSON válido
            return StatusCode(
                (int)response.StatusCode,
                JsonSerializer.Serialize(new { message = $"Error del servidor ({(int)response.StatusCode})", details = rawContent })
            );
        }
        catch (Exception ex)
        {
            Response.ContentType = "application/json";
            return StatusCode(503, JsonSerializer.Serialize(new
            {
                message = "El servidor de IA no está disponible. ¿Está corriendo el Backend?",
                details = ex.Message
            }));
        }
    }

    // ============================================================
    // PRODUCTOS
    // ============================================================

    public async Task<IActionResult> Productos()
    {
        AddAuthorizationHeader();
        try
        {
            var response = await _httpClient.GetAsync($"{_apiBaseUrl}/producto");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var productos = JsonSerializer.Deserialize<List<ProductoViewModel>>(content, options);
                return View(productos);
            }
        }
        catch (Exception ex)
        {
            ViewBag.ErrorMessage = "Error al conectar con la API: " + ex.Message;
        }

        return View(new List<ProductoViewModel>());
    }

    public async Task<IActionResult> Create()
    {
        ViewBag.Categorias = await GetCategoriasAsync();
        ViewBag.Marcas = await GetMarcasAsync();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(IFormCollection form)
    {
        AddAuthorizationHeader();
        try
        {
            var dto = new
            {
                Nombre = form["Nombre"].ToString(),
                Descripcion = form["Descripcion"].ToString(),
                Precio = decimal.Parse(form["Precio"].ToString() ?? "0"),
                Stock = int.Parse(form["Stock"].ToString() ?? "0"),
                IdCategoria = int.Parse(form["IdCategoria"].ToString() ?? "0"),
                IdMarca = int.Parse(form["IdMarca"].ToString() ?? "0"),
                Seccion = form["Seccion"].ToString()
            };

            var jsonContent = new StringContent(JsonSerializer.Serialize(dto), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{_apiBaseUrl}/producto", jsonContent);

            if (response.IsSuccessStatusCode)
            {
                var contentStr = await response.Content.ReadAsStringAsync();
                var createdProduct = JsonSerializer.Deserialize<ProductoViewModel>(contentStr, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                int newId = createdProduct?.IdProducto ?? 0;

                // Subir imágenes si existen
                if (newId > 0 && form.Files.Count > 0)
                {
                    using var formData = new MultipartFormDataContent();
                    foreach (var file in form.Files)
                    {
                        var fileContent = new StreamContent(file.OpenReadStream());
                        fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
                        formData.Add(fileContent, "imagenes", file.FileName);
                    }
                    await _httpClient.PostAsync($"{_apiBaseUrl}/producto/{newId}/imagenes", formData);
                }

                TempData["SuccessMessage"] = "Producto creado exitosamente.";
                return RedirectToAction(nameof(Productos));
            }
            else
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                ViewBag.ErrorMessage = $"Error al crear producto ({response.StatusCode}): {errorBody}";
            }
        }
        catch (Exception ex)
        {
            ViewBag.ErrorMessage = "Excepción al crear producto: " + ex.Message;
        }

        ViewBag.Categorias = await GetCategoriasAsync();
        ViewBag.Marcas = await GetMarcasAsync();
        return View();
    }

    public async Task<IActionResult> Edit(int id)
    {
        AddAuthorizationHeader();
        try
        {
            var response = await _httpClient.GetAsync($"{_apiBaseUrl}/producto/{id}");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var producto = JsonSerializer.Deserialize<ProductoViewModel>(content, options);
                ViewBag.Categorias = await GetCategoriasAsync();
                ViewBag.Marcas = await GetMarcasAsync();
                return View(producto);
            }
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "No se pudo cargar el producto para editar: " + ex.Message;
        }
        return RedirectToAction(nameof(Productos));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, IFormCollection form)
    {
        AddAuthorizationHeader();
        try
        {
            var dto = new
            {
                Nombre = form["Nombre"].ToString(),
                Descripcion = form["Descripcion"].ToString(),
                Precio = decimal.Parse(form["Precio"].ToString() ?? "0"),
                Stock = int.Parse(form["Stock"].ToString() ?? "0"),
                IdCategoria = int.Parse(form["IdCategoria"].ToString() ?? "0"),
                IdMarca = int.Parse(form["IdMarca"].ToString() ?? "0"),
                Seccion = form["Seccion"].ToString()
            };

            var jsonContent = new StringContent(JsonSerializer.Serialize(dto), Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync($"{_apiBaseUrl}/producto/{id}", jsonContent);

            if (response.IsSuccessStatusCode)
            {
                // Subir imágenes extra si enviaron
                if (form.Files.Count > 0)
                {
                    using var formData = new MultipartFormDataContent();
                    foreach (var file in form.Files)
                    {
                        var fileContent = new StreamContent(file.OpenReadStream());
                        fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
                        formData.Add(fileContent, "imagenes", file.FileName);
                    }
                    await _httpClient.PostAsync($"{_apiBaseUrl}/producto/{id}/imagenes", formData);
                }

                TempData["SuccessMessage"] = "Producto actualizado exitosamente.";
                return RedirectToAction(nameof(Productos));
            }
            else
            {
                ViewBag.ErrorMessage = "Error al actualizar producto. Código: " + response.StatusCode;
            }
        }
        catch (Exception ex)
        {
            ViewBag.ErrorMessage = "Excepción al actualizar producto: " + ex.Message;
        }
        return RedirectToAction(nameof(Edit), new { id = id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        AddAuthorizationHeader();
        try
        {
            var response = await _httpClient.DeleteAsync($"{_apiBaseUrl}/producto/{id}");
            TempData[response.IsSuccessStatusCode ? "SuccessMessage" : "ErrorMessage"] =
                response.IsSuccessStatusCode ? "Producto desactivado exitosamente." : "Error al desactivar el producto.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "Excepción: " + ex.Message;
        }
        return RedirectToAction(nameof(Productos));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleStatus(int id)
    {
        AddAuthorizationHeader();
        try
        {
            var response = await _httpClient.PutAsync($"{_apiBaseUrl}/producto/{id}/toggle-status", null);
            TempData[response.IsSuccessStatusCode ? "SuccessMessage" : "ErrorMessage"] =
                response.IsSuccessStatusCode ? "Estado del producto cambiado exitosamente." : "Error al cambiar estado del producto.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "Excepción: " + ex.Message;
        }
        return RedirectToAction(nameof(Productos));
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteImage(int idProducto, int idImagen)
    {
        AddAuthorizationHeader();
        try
        {
            var response = await _httpClient.DeleteAsync($"{_apiBaseUrl}/producto/{idProducto}/imagen/{idImagen}");
            TempData[response.IsSuccessStatusCode ? "SuccessMessage" : "ErrorMessage"] =
                response.IsSuccessStatusCode ? "Imagen eliminada exitosamente." : "Error al eliminar la imagen.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "Excepción: " + ex.Message;
        }
        return RedirectToAction(nameof(Edit), new { id = idProducto });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetPrimaryImage(int idProducto, int idImagen)
    {
        AddAuthorizationHeader();
        try
        {
            var response = await _httpClient.PutAsync($"{_apiBaseUrl}/producto/{idProducto}/imagen/{idImagen}/principal", null);
            TempData[response.IsSuccessStatusCode ? "SuccessMessage" : "ErrorMessage"] =
                response.IsSuccessStatusCode ? "Imagen establecida como principal exitosamente." : "Error al establecer imagen principal.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "Excepción: " + ex.Message;
        }
        return RedirectToAction(nameof(Edit), new { id = idProducto });
    }

    // ============================================================
    // SEDES
    // ============================================================

    public async Task<IActionResult> Sedes()
    {
        AddAuthorizationHeader();
        try
        {
            var response = await _httpClient.GetAsync($"{_apiBaseUrl}/sede");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var sedes = JsonSerializer.Deserialize<List<SedeViewModel>>(content, opts) ?? new List<SedeViewModel>();
                ViewBag.Sedes = sedes;
            }
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "No se pudieron cargar las sedes: " + ex.Message;
        }
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CrearSede(IFormCollection form)
    {
        AddAuthorizationHeader();
        try
        {
            var body = new StringContent(
                JsonSerializer.Serialize(new { 
                    nombre = form["Nombre"].ToString(), 
                    ciudad = form["Ciudad"].ToString(),
                    direccion = form["Direccion"].ToString(),
                    telefono = form["Telefono"].ToString(),
                    activa = true
                }),
                Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{_apiBaseUrl}/sede", body);
            if (response.IsSuccessStatusCode)
                TempData["SuccessMessage"] = $"Sede «{form["Nombre"]}» creada exitosamente.";
            else
            {
                var err = await response.Content.ReadAsStringAsync();
                TempData["ErrorMessage"] = $"Error al crear sede: {err}";
            }
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "Error de red: " + ex.Message;
        }
        return RedirectToAction(nameof(Sedes));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditarSede(int id, IFormCollection form)
    {
        AddAuthorizationHeader();
        try
        {
            // Nota: Para mantener simplicidad, usamos 'activa' de hidden input
            bool activa = form["Activa"].ToString().ToLower() == "true";
            
            var body = new StringContent(
                JsonSerializer.Serialize(new { 
                    nombre = form["Nombre"].ToString(), 
                    ciudad = form["Ciudad"].ToString(),
                    direccion = form["Direccion"].ToString(),
                    telefono = form["Telefono"].ToString(),
                    activa = activa
                }),
                Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync($"{_apiBaseUrl}/sede/{id}", body);
            if (response.IsSuccessStatusCode)
                TempData["SuccessMessage"] = $"Sede actualizada exitosamente.";
            else
            {
                var err = await response.Content.ReadAsStringAsync();
                TempData["ErrorMessage"] = $"Error al actualizar sede: {err}";
            }
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "Error de red: " + ex.Message;
        }
        return RedirectToAction(nameof(Sedes));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleSede(int id)
    {
        AddAuthorizationHeader();
        try
        {
            var response = await _httpClient.PutAsync($"{_apiBaseUrl}/sede/{id}/toggle-status", null);
            if (response.IsSuccessStatusCode)
                TempData["SuccessMessage"] = "Estado de sede cambiado correctamente.";
            else
                TempData["ErrorMessage"] = "No se pudo cambiar el estado.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "Excepción: " + ex.Message;
        }
        return RedirectToAction(nameof(Sedes));
    }

    // ============================================================
    // CATEGORÍAS
    // ============================================================


    public async Task<IActionResult> Categorias()
    {
        AddAuthorizationHeader();
        try
        {
            var response = await _httpClient.GetAsync($"{_apiBaseUrl}/categoria");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var cats = JsonSerializer.Deserialize<List<CategoriaViewModel>>(content, opts) ?? new List<CategoriaViewModel>();
                ViewBag.Categorias = cats;
            }
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "No se pudieron cargar las categorías: " + ex.Message;
        }
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CrearCategoria(IFormCollection form)
    {
        AddAuthorizationHeader();
        try
        {
            var body = new StringContent(
                JsonSerializer.Serialize(new { nombre = form["Nombre"].ToString(), descripcion = form["Descripcion"].ToString() }),
                Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{_apiBaseUrl}/categoria", body);
            if (response.IsSuccessStatusCode)
                TempData["SuccessMessage"] = $"Categoría «{form["Nombre"]}» creada exitosamente.";
            else
            {
                var err = await response.Content.ReadAsStringAsync();
                TempData["ErrorMessage"] = $"Error al crear categoría: {err}";
            }
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "Error de red: " + ex.Message;
        }
        return RedirectToAction(nameof(Categorias));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EliminarCategoria(int id)
    {
        AddAuthorizationHeader();
        try
        {
            var response = await _httpClient.DeleteAsync($"{_apiBaseUrl}/categoria/{id}");
            if (response.IsSuccessStatusCode)
                TempData["SuccessMessage"] = "Categoría eliminada correctamente.";
            else
            {
                var err = await response.Content.ReadAsStringAsync();
                TempData["ErrorMessage"] = $"No se pudo eliminar: {err}";
            }
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "Error de red: " + ex.Message;
        }
        return RedirectToAction(nameof(Categorias));
    }

    // ============================================================
    // MARCAS
    // ============================================================

    [HttpGet]
    public async Task<IActionResult> Marcas()
    {
        var marcas = new List<MarcaViewModel>();
        try
        {
            AddAuthorizationHeader();
            var response = await _httpClient.GetAsync($"{_apiBaseUrl}/marcas");
            if (response.IsSuccessStatusCode)
            {
                var jsonString = await response.Content.ReadAsStringAsync();
                marcas = JsonSerializer.Deserialize<List<MarcaViewModel>>(jsonString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<MarcaViewModel>();
            }
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "No se pudieron cargar las marcas: " + ex.Message;
        }
        return View(marcas);
    }

    [HttpGet]
    public IActionResult CrearMarca()
    {
        return View(new MarcaViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CrearMarca(MarcaViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        try
        {
            if (model.LogoImagen != null && model.LogoImagen.Length > 0)
            {
                var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);
                var uniqueFileName = Guid.NewGuid().ToString() + "_" + model.LogoImagen.FileName;
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                    await model.LogoImagen.CopyToAsync(fileStream);
                model.LogoUrl = $"/uploads/{uniqueFileName}";
            }

            AddAuthorizationHeader();
            var content = new StringContent(JsonSerializer.Serialize(new { nombre = model.Nombre, descripcion = model.Descripcion, logoUrl = model.LogoUrl }), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{_apiBaseUrl}/marcas", content);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = $"Marca \"{model.Nombre}\" creada exitosamente.";
                return RedirectToAction(nameof(Marcas));
            }
            ModelState.AddModelError(string.Empty, "Error en el servidor al crear la marca.");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, "Error de red: " + ex.Message);
        }
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> EditarMarca(int id)
    {
        try
        {
            AddAuthorizationHeader();
            var response = await _httpClient.GetAsync($"{_apiBaseUrl}/marcas/{id}");
            if (response.IsSuccessStatusCode)
            {
                var jsonString = await response.Content.ReadAsStringAsync();
                var marca = JsonSerializer.Deserialize<MarcaViewModel>(jsonString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return View(marca);
            }
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "Error de red: " + ex.Message;
        }
        return RedirectToAction(nameof(Marcas));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditarMarca(int id, MarcaViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        try
        {
            if (model.LogoImagen != null && model.LogoImagen.Length > 0)
            {
                var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);
                var uniqueFileName = Guid.NewGuid().ToString() + "_" + model.LogoImagen.FileName;
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                    await model.LogoImagen.CopyToAsync(fileStream);
                model.LogoUrl = $"/uploads/{uniqueFileName}";
            }

            AddAuthorizationHeader();
            var content = new StringContent(JsonSerializer.Serialize(new { idMarca = model.IdMarca, nombre = model.Nombre, descripcion = model.Descripcion, logoUrl = model.LogoUrl }), Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync($"{_apiBaseUrl}/marcas/{id}", content);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Marca actualizada exitosamente.";
                return RedirectToAction(nameof(Marcas));
            }
            ModelState.AddModelError(string.Empty, "Error en el servidor al actualizar la marca.");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, "Error de red: " + ex.Message);
        }
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EliminarMarca(int id)
    {
        try
        {
            AddAuthorizationHeader();
            var response = await _httpClient.DeleteAsync($"{_apiBaseUrl}/marcas/{id}");
            TempData[response.IsSuccessStatusCode ? "SuccessMessage" : "ErrorMessage"] =
                response.IsSuccessStatusCode ? "Marca eliminada exitosamente." : "No se pudo eliminar la marca.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "Error de red: " + ex.Message;
        }
        return RedirectToAction(nameof(Marcas));
    }

    // ============================================================
    // USUARIOS
    // ============================================================

    [HttpGet]
    public async Task<IActionResult> Usuarios()
    {
        var usuarios = new List<UsuarioViewModel>();
        try
        {
            AddAuthorizationHeader();
            var response = await _httpClient.GetAsync($"{_apiBaseUrl}/usuario");
            if (response.IsSuccessStatusCode)
            {
                var jsonString = await response.Content.ReadAsStringAsync();
                usuarios = JsonSerializer.Deserialize<List<UsuarioViewModel>>(jsonString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<UsuarioViewModel>();
            }
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "No se pudieron cargar los usuarios: " + ex.Message;
        }
        return View(usuarios);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleUsuario(int id, bool activar)
    {
        try
        {
            AddAuthorizationHeader();
            var content = new StringContent(JsonSerializer.Serialize(new { activo = activar }), Encoding.UTF8, "application/json");
            var response = await _httpClient.PatchAsync($"{_apiBaseUrl}/usuario/{id}/estado", content);

            TempData[response.IsSuccessStatusCode ? "SuccessMessage" : "ErrorMessage"] =
                response.IsSuccessStatusCode ? "Estado de usuario actualizado exitosamente." : "Error al actualizar el estado del usuario.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "Excepción: " + ex.Message;
        }
        return RedirectToAction(nameof(Usuarios));
    }

    private async Task<IEnumerable<SelectListItem>> GetRolesAsync()
    {
        var roles = new List<SelectListItem>();
        AddAuthorizationHeader();
        var response = await _httpClient.GetAsync($"{_apiBaseUrl}/usuario/roles");
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            var arr = JsonSerializer.Deserialize<JsonElement>(content);
            foreach (var r in arr.EnumerateArray())
            {
                roles.Add(new SelectListItem { Value = r.GetProperty("id_rol").GetInt32().ToString(), Text = r.GetProperty("nombre_rol").GetString() });
            }
        }
        return roles;
    }

    private async Task<IEnumerable<SelectListItem>> GetSedesAsync()
    {
        var sedes = new List<SelectListItem>();
        AddAuthorizationHeader();
        var response = await _httpClient.GetAsync($"{_apiBaseUrl}/sede");
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var list = JsonSerializer.Deserialize<List<SedeViewModel>>(content, opts) ?? new List<SedeViewModel>();
            foreach (var s in list.Where(x => x.Activa))
            {
                sedes.Add(new SelectListItem { Value = s.IdSede.ToString(), Text = $"{s.Nombre} ({s.Ciudad})" });
            }
        }
        return sedes;
    }

    [HttpGet]
    public async Task<IActionResult> CrearUsuario()
    {
        var model = new UsuarioCreateViewModel();
        model.RolesDisponibles = await GetRolesAsync();
        model.SedesDisponibles = await GetSedesAsync();
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CrearUsuario(UsuarioCreateViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.RolesDisponibles = await GetRolesAsync();
            model.SedesDisponibles = await GetSedesAsync();
            return View(model);
        }

        try
        {
            AddAuthorizationHeader();
            var content = new StringContent(JsonSerializer.Serialize(new
            {
                nombre = model.Nombre,
                apellido = model.Apellido,
                email = model.Email,
                password = model.Password,
                telefono = model.Telefono,
                idRol = model.IdRol,
                idSede = model.IdSede
            }), Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{_apiBaseUrl}/usuario", content);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = $"Usuario creado exitosamente con el Rol especificado.";
                return RedirectToAction(nameof(Usuarios));
            }

            var errorResponse = await response.Content.ReadAsStringAsync();
            ModelState.AddModelError(string.Empty, "Error del servidor: " + errorResponse);
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, "Error de red: " + ex.Message);
        }

        model.RolesDisponibles = await GetRolesAsync();
        model.SedesDisponibles = await GetSedesAsync();
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> EditarUsuario(int id)
    {
        try
        {
            AddAuthorizationHeader();
            var response = await _httpClient.GetAsync($"{_apiBaseUrl}/usuario/{id}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var u = JsonSerializer.Deserialize<JsonElement>(content);

                var model = new UsuarioEditViewModel
                {
                    IdUsuario = u.GetProperty("idUsuario").GetInt32(),
                    Nombre = u.GetProperty("nombre").GetString(),
                    Apellido = u.GetProperty("apellido").GetString(),
                    Email = u.GetProperty("email").GetString(),
                    Telefono = u.TryGetProperty("telefono", out JsonElement telEl) ? telEl.GetString() : "",
                    IdRol = u.GetProperty("idRol").GetInt32(),
                    IdSede = u.TryGetProperty("idSede", out JsonElement sedeEl) && sedeEl.ValueKind != JsonValueKind.Null ? sedeEl.GetInt32() : null,
                    RolesDisponibles = await GetRolesAsync(),
                    SedesDisponibles = await GetSedesAsync()
                };
                return View(model);
            }
            TempData["ErrorMessage"] = "No se pudo cargar la información del usuario.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "Error de red: " + ex.Message;
        }

        return RedirectToAction(nameof(Usuarios));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditarUsuario(int id, UsuarioEditViewModel model)
    {
        if (id != model.IdUsuario) return NotFound();

        if (!ModelState.IsValid)
        {
            model.RolesDisponibles = await GetRolesAsync();
            model.SedesDisponibles = await GetSedesAsync();
            return View(model);
        }

        try
        {
            AddAuthorizationHeader();
            var content = new StringContent(JsonSerializer.Serialize(new
            {
                nombre = model.Nombre,
                apellido = model.Apellido,
                telefono = model.Telefono,
                idRol = model.IdRol,
                idSede = model.IdSede
            }), Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync($"{_apiBaseUrl}/usuario/{id}", content);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Usuario actualizado exitosamente.";
                return RedirectToAction(nameof(Usuarios));
            }

            var errorResponse = await response.Content.ReadAsStringAsync();
            ModelState.AddModelError(string.Empty, "Error del servidor: " + errorResponse);
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, "Error de red: " + ex.Message);
        }

        model.RolesDisponibles = await GetRolesAsync();
        model.SedesDisponibles = await GetSedesAsync();
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EliminarUsuario(int id)
    {
        try
        {
            AddAuthorizationHeader();
            var response = await _httpClient.DeleteAsync($"{_apiBaseUrl}/usuario/{id}");

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Usuario eliminado permanentemente.";
            }
            else
            {
                var errorResponse = await response.Content.ReadAsStringAsync();
                TempData["ErrorMessage"] = "No se pudo eliminar el usuario. " + errorResponse;
            }
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "Error de red: " + ex.Message;
        }

        return RedirectToAction(nameof(Usuarios));
    }

    // ============================================================
    // PROVEEDORES
    // ============================================================

    [HttpGet]
    public async Task<IActionResult> Proveedores()
    {
        var proveedores = new List<ProveedorViewModel>();
        try
        {
            AddAuthorizationHeader();
            var response = await _httpClient.GetAsync($"{_apiBaseUrl}/proveedor");
            if (response.IsSuccessStatusCode)
            {
                var jsonString = await response.Content.ReadAsStringAsync();
                proveedores = JsonSerializer.Deserialize<List<ProveedorViewModel>>(jsonString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<ProveedorViewModel>();
            }
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "No se pudieron cargar los proveedores: " + ex.Message;
        }
        return View(proveedores);
    }

    [HttpGet]
    public IActionResult CrearProveedor() => View(new ProveedorCreateUpdateViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CrearProveedor(ProveedorCreateUpdateViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        try
        {
            AddAuthorizationHeader();
            var content = new StringContent(JsonSerializer.Serialize(new
            {
                nombre = model.Nombre,
                contacto = model.Contacto,
                telefono = model.Telefono,
                email = model.Email
            }), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{_apiBaseUrl}/proveedor", content);
            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = $"Proveedor \"{model.Nombre}\" creado exitosamente.";
                return RedirectToAction(nameof(Proveedores));
            }
            ModelState.AddModelError(string.Empty, "Error en el servidor al crear el proveedor.");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, "Error de red: " + ex.Message);
        }
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> EditarProveedor(int id)
    {
        try
        {
            AddAuthorizationHeader();
            var response = await _httpClient.GetAsync($"{_apiBaseUrl}/proveedor/{id}");
            if (response.IsSuccessStatusCode)
            {
                var jsonString = await response.Content.ReadAsStringAsync();
                var model = JsonSerializer.Deserialize<ProveedorCreateUpdateViewModel>(jsonString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return View(model);
            }
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "Error de red: " + ex.Message;
        }
        return RedirectToAction(nameof(Proveedores));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditarProveedor(int id, ProveedorCreateUpdateViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        try
        {
            AddAuthorizationHeader();
            var content = new StringContent(JsonSerializer.Serialize(new
            {
                nombre = model.Nombre,
                contacto = model.Contacto,
                telefono = model.Telefono,
                email = model.Email
            }), Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync($"{_apiBaseUrl}/proveedor/{id}", content);
            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Proveedor actualizado exitosamente.";
                return RedirectToAction(nameof(Proveedores));
            }
            ModelState.AddModelError(string.Empty, "Error al actualizar el proveedor.");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, "Error de red: " + ex.Message);
        }
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleProveedor(int id)
    {
        try
        {
            AddAuthorizationHeader();
            var response = await _httpClient.PatchAsync($"{_apiBaseUrl}/proveedor/{id}/toggle", null);
            TempData[response.IsSuccessStatusCode ? "SuccessMessage" : "ErrorMessage"] =
                response.IsSuccessStatusCode ? "Estado del proveedor cambiado exitosamente." : "Error al cambiar estado del proveedor.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "Excepción: " + ex.Message;
        }
        return RedirectToAction(nameof(Proveedores));
    }

    // ============================================================
    // INVENTARIO ERP
    // ============================================================

    [HttpGet]
    public async Task<IActionResult> Inventario(int? sedeId = null)
    {
        var inventario  = new List<InventarioSedeViewModel>();
        var historial   = new List<AbastecimientoViewModel>();
        var proveedores = new List<ProveedorViewModel>();
        var productos   = new List<ProductoViewModel>();
        var sedes       = new List<SedeViewModel>();

        try
        {
            AddAuthorizationHeader();
            var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            // Stock actual
            var urlInv = sedeId.HasValue ? $"{_apiBaseUrl}/inventario?idSede={sedeId}" : $"{_apiBaseUrl}/inventario";
            var rInv = await _httpClient.GetAsync(urlInv);
            if (rInv.IsSuccessStatusCode)
                inventario = JsonSerializer.Deserialize<List<InventarioSedeViewModel>>(await rInv.Content.ReadAsStringAsync(), opts) ?? inventario;

            // Historial (últimos 20)
            var rHist = await _httpClient.GetAsync($"{_apiBaseUrl}/inventario/historial");
            if (rHist.IsSuccessStatusCode)
                historial = JsonSerializer.Deserialize<List<AbastecimientoViewModel>>(await rHist.Content.ReadAsStringAsync(), opts) ?? historial;

            // Catálogos para formularios
            var rProv = await _httpClient.GetAsync($"{_apiBaseUrl}/proveedor");
            if (rProv.IsSuccessStatusCode)
                proveedores = JsonSerializer.Deserialize<List<ProveedorViewModel>>(await rProv.Content.ReadAsStringAsync(), opts) ?? proveedores;

            var rProd = await _httpClient.GetAsync($"{_apiBaseUrl}/producto");
            if (rProd.IsSuccessStatusCode)
                productos = JsonSerializer.Deserialize<List<ProductoViewModel>>(await rProd.Content.ReadAsStringAsync(), opts) ?? productos;

            var rSedes = await _httpClient.GetAsync($"{_apiBaseUrl}/sede");
            if (rSedes.IsSuccessStatusCode)
                sedes = JsonSerializer.Deserialize<List<SedeViewModel>>(await rSedes.Content.ReadAsStringAsync(), opts) ?? sedes;
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "Error cargando datos de inventario: " + ex.Message;
        }

        ViewBag.Inventario   = inventario;
        ViewBag.Historial    = historial;
        ViewBag.Proveedores  = proveedores;
        ViewBag.Productos    = productos;
        ViewBag.Sedes        = sedes;
        ViewBag.SedeIdActual = sedeId;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RegistrarEntrada(EntradaInventarioViewModel model)
    {
        try
        {
            AddAuthorizationHeader();
            var content = new StringContent(JsonSerializer.Serialize(new
            {
                idProducto    = model.IdProducto,
                idProveedor   = model.IdProveedor,
                idSede        = model.IdSede,
                cantidad      = model.Cantidad,
                costoUnitario = model.CostoUnitario,
                notas         = model.Notas
            }), Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{_apiBaseUrl}/inventario/entrada", content);
            TempData[response.IsSuccessStatusCode ? "SuccessMessage" : "ErrorMessage"] =
                response.IsSuccessStatusCode ? "Entrada de mercadería registrada y stock actualizado." : "Error al registrar la entrada.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "Error de red: " + ex.Message;
        }
        return RedirectToAction(nameof(Inventario));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AjustarStock(int idProducto, int idSede, int cantidad, int umbral)
    {
        try
        {
            AddAuthorizationHeader();
            var content = new StringContent(JsonSerializer.Serialize(new
            {
                idProducto    = idProducto,
                idSede        = idSede,
                cantidadNueva = cantidad,
                umbralMinimo  = umbral
            }), Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync($"{_apiBaseUrl}/inventario/ajustar", content);
            TempData[response.IsSuccessStatusCode ? "SuccessMessage" : "ErrorMessage"] =
                response.IsSuccessStatusCode ? "Stock ajustado correctamente." : "Error al ajustar el stock.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "Error: " + ex.Message;
        }
        return RedirectToAction(nameof(Inventario));
    }

    // ============================================================
    // BUSCADOR GLOBAL (AJAX)
    // ============================================================
    [HttpGet]
    public async Task<IActionResult> BuscarGlobal(string q, string filter = "all")
    {
        if (string.IsNullOrWhiteSpace(q)) return Json(new object[] { });
        
        try
        {
            AddAuthorizationHeader();
            var response = await _httpClient.GetAsync($"{_apiBaseUrl}/search?q={Uri.EscapeDataString(q)}&filter={filter}");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return Content(content, "application/json");
            }
        }
        catch { /* Ignore error on auto-complete */ }
        
        return Json(new object[] { });
    }

    // ============================================================
    // HELPERS PRIVADOS
    // ============================================================

    private async Task<List<CategoriaViewModel>> GetCategoriasAsync()
    {
        try
        {
            AddAuthorizationHeader();
            var response = await _httpClient.GetAsync($"{_apiBaseUrl}/categoria");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                return JsonSerializer.Deserialize<List<CategoriaViewModel>>(content, opts) ?? new List<CategoriaViewModel>();
            }
        }
        catch { }
        return new List<CategoriaViewModel>();
    }

    private async Task<List<MarcaDropdownViewModel>> GetMarcasAsync()
    {
        try
        {
            AddAuthorizationHeader();
            var response = await _httpClient.GetAsync($"{_apiBaseUrl}/marcas");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                return JsonSerializer.Deserialize<List<MarcaDropdownViewModel>>(content, opts) ?? new List<MarcaDropdownViewModel>();
            }
        }
        catch { }
        return new List<MarcaDropdownViewModel>();
    }
}
