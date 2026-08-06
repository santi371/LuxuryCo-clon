using LuxuryCo.Front.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using System.Text.Json;

namespace LuxuryCo.Front.Controllers
{
    public class AccountController : Controller
    {
        private readonly Microsoft.AspNetCore.Hosting.IWebHostEnvironment _env;
        private readonly LuxuryCo.Database.Data.LuxuryCoDbContext _context;

        public AccountController(
            Microsoft.AspNetCore.Hosting.IWebHostEnvironment env,
            LuxuryCo.Database.Data.LuxuryCoDbContext context)
        {
            _env = env;
            _context = context;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                using var client = new System.Net.Http.HttpClient();
                var requestBody = new StringContent(
                    JsonSerializer.Serialize(new { email = model.Email, password = model.Password }),
                    System.Text.Encoding.UTF8, "application/json");

                var response = await client.PostAsync((Environment.GetEnvironmentVariable("API_BASE_URL") ?? "http://localhost:5137") + "/api/auth/login", requestBody);

                if (response.IsSuccessStatusCode)
                {
                    // Deserializar la respuesta para obtener el Rol del usuario y el Token
                    var responseJson = await response.Content.ReadAsStringAsync();
                    string userRole = "CLIENTE";
                    string token = "";
                    try
                    {
                        using var doc = JsonDocument.Parse(responseJson);
                        if (doc.RootElement.TryGetProperty("usuario", out var usuarioEl))
                        {
                            userRole = usuarioEl.GetProperty("rol").GetString() ?? "CLIENTE";
                        }
                        if (doc.RootElement.TryGetProperty("token", out var tokenEl))
                        {
                            token = tokenEl.GetString() ?? "";
                        }
                    }
                    catch { /* Si falla el parse del rol, usar CLIENTE por defecto */ }

                    if (!string.IsNullOrEmpty(token))
                    {
                        Response.Cookies.Append("jwt_token", token, new CookieOptions 
                        { 
                            HttpOnly = true, 
                            Expires = DateTimeOffset.UtcNow.AddDays(7),
                            // SameSite=Lax permite que la cookie viaje en peticiones del mismo sitio
                            // Secure=false en desarrollo para evitar que el navegador rechace la cookie en localhost
                            Secure = Request.IsHttps,
                            SameSite = SameSiteMode.Lax
                        });
                    }

                    // Crear claims incluyendo el Rol para que User.IsInRole() funcione
                    var claims = new List<System.Security.Claims.Claim>
                    {
                        new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Email, model.Email),
                        new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Name, model.Email),
                        new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, userRole)
                    };

                    var claimsIdentity = new System.Security.Claims.ClaimsIdentity(
                        claims, Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme);

                    var authProperties = new AuthenticationProperties
                    {
                        IsPersistent = true,
                        ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
                    };

                    await HttpContext.SignInAsync(
                        Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme,
                        new System.Security.Claims.ClaimsPrincipal(claimsIdentity),
                        authProperties);

                    // Redirigir a panel admin si es administrador
                    if (userRole == "ADMIN")
                        return RedirectToAction("Index", "Admin");

                    return RedirectToAction("Profile", new { email = model.Email });
                }

                var errContent = await response.Content.ReadAsStringAsync();
                ModelState.AddModelError(string.Empty, "Correo o contraseña incorrectos. " + errContent);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Error de conexión con el Backend: Asegúrate de que tu proyecto 'LuxuryCo.Back' se esté ejecutando al mismo tiempo en el puerto 7066. Detalles:" + ex.Message);
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var errors = string.Join(" ", ModelState.Values
                                    .SelectMany(v => v.Errors)
                                    .Select(e => e.ErrorMessage));
                TempData["ErrorMessage"] = string.IsNullOrEmpty(errors) 
                    ? "Por favor, verifica que los datos cumplan con el formato correcto." 
                    : errors;
                return View(model);
            }

            try
            {
                using var client = new System.Net.Http.HttpClient();
                var requestObj = new
                {
                    nombre = model.Nombre,
                    apellido = model.Apellido,
                    email = model.Email,
                    password = model.Password,
                    telefono = model.Telefono
                };
                var content = new StringContent(JsonSerializer.Serialize(requestObj), System.Text.Encoding.UTF8, "application/json");

                var response = await client.PostAsync((Environment.GetEnvironmentVariable("API_BASE_URL") ?? "http://localhost:5137") + "/api/auth/register", content);

                if (response.IsSuccessStatusCode)
                {
                    var contentString = await response.Content.ReadAsStringAsync();
                    if (contentString.Contains("REQUIRES_EMAIL_CONFIRMATION"))
                    {
                        return RedirectToAction("CheckEmail");
                    }

                    TempData["SuccessMessage"] = "¡Cuenta creada exitosamente! Por favor, inicia sesión.";
                    return RedirectToAction("Login");
                }

                var err = await response.Content.ReadAsStringAsync();
                TempData["ErrorMessage"] = "Error al completar el registro: " + err;
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error de conexión con el Backend: " + ex.Message;
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult CheckEmail()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login");
            }

            string email = User.Identity.Name;

            var model = new ProfileViewModel
            {
                Nombre = "Cargando...",
                Apellido = "",
                Email = email,
                FotoPerfilUrl = ""
            };

            try
            {
                var usuario = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.FirstOrDefaultAsync(
                    _context.Usuarios, u => u.email == email);

                if (usuario != null)
                {
                    model.Nombre = usuario.nombre;
                    model.Apellido = usuario.apellido;
                    model.Email = usuario.email;
                    model.FotoPerfilUrl = usuario.foto_perfil_url ?? "";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error de conexión a DB: {ex.Message}");
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> UploadProfilePicture(IFormFile file)
        {
            if (!User.Identity.IsAuthenticated) return Json(new { success = false, message = "No autenticado" });

            string email = User.Identity.Name;

            if (file == null || file.Length == 0) return Json(new { success = false, message = "Archivo vacío" });

            try
            {
                var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }

                var relativeUrl = $"/uploads/{uniqueFileName}";

                try
                {
                    var usuario = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.FirstOrDefaultAsync(
                        _context.Usuarios, u => u.email == email);

                    if (usuario != null)
                    {
                        usuario.foto_perfil_url = relativeUrl;
                        _context.Usuarios.Update(usuario);
                        await _context.SaveChangesAsync();
                    }
                }
                catch (Exception ex)
                {
                    var errorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                    return Json(new { success = false, message = "Error al guardar en base de datos: " + errorMessage });
                }

                return Json(new { success = true, url = relativeUrl });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme);
            Response.Cookies.Delete("jwt_token");
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return RedirectToAction("Login");
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Por favor, ingresa un correo válido.";
                return RedirectToAction("Login");
            }

            using var client = new System.Net.Http.HttpClient();
            var content = new StringContent(JsonSerializer.Serialize(new { email = model.Email }), System.Text.Encoding.UTF8, "application/json");

            var response = await client.PostAsync((Environment.GetEnvironmentVariable("API_BASE_URL") ?? "http://localhost:5137") + "/api/auth/forgot-password", content);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Te hemos enviado un enlace de recuperación. Por favor, revisa tu correo electrónico.";
                return RedirectToAction("Login");
            }

            var errContent = await response.Content.ReadAsStringAsync();
            TempData["ErrorMessage"] = "Error al procesar la solicitud. " + errContent;
            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult ResetPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            using var client = new System.Net.Http.HttpClient();
            var content = new StringContent(JsonSerializer.Serialize(new
            {
                accessToken = model.AccessToken,
                refreshToken = model.RefreshToken,
                newPassword = model.NewPassword
            }), System.Text.Encoding.UTF8, "application/json");

            var response = await client.PostAsync((Environment.GetEnvironmentVariable("API_BASE_URL") ?? "http://localhost:5137") + "/api/auth/reset-password", content);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Tu contraseña ha sido restablecida. Inicia sesión con tu nueva contraseña.";
                return RedirectToAction("Login");
            }

            var errContent = await response.Content.ReadAsStringAsync();
            ModelState.AddModelError(string.Empty, "Error al restablecer la contraseña. " + errContent);
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> CompleteSecureRegistration([FromBody] CompleteSecureRegRequest model)
        {
            if (model == null || string.IsNullOrEmpty(model.Token) || string.IsNullOrEmpty(model.Password))
                return BadRequest(new { message = "Datos inválidos." });

            try
            {
                using var client = new System.Net.Http.HttpClient();
                var content = new StringContent(JsonSerializer.Serialize(model), System.Text.Encoding.UTF8, "application/json");

                // Enviar la petición al backend (puerto configurado de Render o localhost en desarrollo)
                var response = await client.PostAsync((Environment.GetEnvironmentVariable("API_BASE_URL") ?? "https://localhost:7066") + "/api/auth/complete-secure-registration", content);

                if (response.IsSuccessStatusCode)
                {
                    // Obtener token y rol del backend y loguear automáticamente
                    var responseJson = await response.Content.ReadAsStringAsync();
                    string userRole = "CLIENTE";
                    string token = "";
                    string email = "";
                    try
                    {
                        using var doc = JsonDocument.Parse(responseJson);
                        if (doc.RootElement.TryGetProperty("usuario", out var usuarioEl))
                        {
                            userRole = usuarioEl.GetProperty("rol").GetString() ?? "CLIENTE";
                            email = usuarioEl.GetProperty("email").GetString() ?? "";
                        }
                        if (doc.RootElement.TryGetProperty("token", out var tokenEl))
                        {
                            token = tokenEl.GetString() ?? "";
                        }
                    }
                    catch { /* ignorar */ }

                    if (!string.IsNullOrEmpty(token))
                    {
                        Response.Cookies.Append("jwt_token", token, new CookieOptions 
                        { 
                            HttpOnly = true, 
                            Expires = DateTimeOffset.UtcNow.AddDays(7),
                            Secure = Request.IsHttps,
                            SameSite = SameSiteMode.Lax
                        });

                        var claims = new List<System.Security.Claims.Claim>
                        {
                            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Email, email),
                            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Name, email),
                            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, userRole)
                        };

                        var claimsIdentity = new System.Security.Claims.ClaimsIdentity(
                            claims, Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme);

                        await HttpContext.SignInAsync(
                            Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme,
                            new System.Security.Claims.ClaimsPrincipal(claimsIdentity),
                            new AuthenticationProperties { IsPersistent = true });
                    }

                    return Ok(new { success = true });
                }

                var errContent = await response.Content.ReadAsStringAsync();
                return BadRequest(new { message = errContent });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error interno: " + ex.Message });
            }
        }
    }

    public class CompleteSecureRegRequest
    {
        public string Token { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}

