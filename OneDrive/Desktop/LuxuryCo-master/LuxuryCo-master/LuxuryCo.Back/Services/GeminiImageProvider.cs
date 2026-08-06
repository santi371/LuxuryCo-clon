using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace LuxuryCo.Back.Services;

/// <summary>
/// Proveedor de generación de imágenes usando la API de Gemini Imagen (imagen-3.0-generate-002).
/// Implementa IImageProvider para integrarse en el ImageProviderRouter como fallback de nivel 3.
/// </summary>
public class GeminiImageProvider : IImageProvider
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;
    private readonly ILogger<GeminiImageProvider> _logger;
    private readonly string _apiKey;
    private readonly string _model;
    private readonly string _uploadDir;

    public string ProviderName => "GeminiImagen";

    public GeminiImageProvider(
        HttpClient httpClient,
        IConfiguration config,
        ILogger<GeminiImageProvider> logger)
    {
        _httpClient = httpClient;
        _config = config;
        _logger = logger;

        _apiKey = config["Gemini:ApiKey"] ?? Environment.GetEnvironmentVariable("gemini__key") ?? string.Empty;
        // Modelo de generación de imágenes de Gemini
        _model = config["Gemini:ImageModel"] ?? "imagen-3.0-generate-002";
        _uploadDir = config["Storage:LocalPath"] ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "generated");
    }

    public async Task<ImageGenerationProviderResponse> GenerateImageAsync(
        string optimizedPrompt,
        string negativePrompt,
        int seed)
    {
        var response = new ImageGenerationProviderResponse();
        var stopwatch = Stopwatch.StartNew();

        try
        {
            if (string.IsNullOrWhiteSpace(_apiKey))
            {
                throw new InvalidOperationException("Gemini API Key no configurada. Agrega Gemini:ApiKey en appsettings.");
            }

            // Construir el request body para Imagen
            var requestBody = new
            {
                instances = new[]
                {
                    new
                    {
                        prompt = optimizedPrompt
                    }
                },
                parameters = new
                {
                    sampleCount = 1,
                    seed = seed,
                    negativePrompt = negativePrompt,
                    aspectRatio = "1:1",
                    safetyFilterLevel = "block_some",
                    personGeneration = "dont_allow"
                }
            };

            var url = $"https://generativelanguage.googleapis.com/v1beta/models/{_model}:generateImages?key={_apiKey}";

            using var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Content = new StringContent(
                JsonSerializer.Serialize(requestBody),
                Encoding.UTF8,
                "application/json");

            _logger.LogInformation("Llamando a Gemini Imagen API con prompt: {Prompt}", optimizedPrompt);
            var httpResponse = await _httpClient.SendAsync(request);
            stopwatch.Stop();

            if (!httpResponse.IsSuccessStatusCode)
            {
                var errorBody = await httpResponse.Content.ReadAsStringAsync();
                _logger.LogWarning("Gemini Imagen falló ({Status}): {Error}", httpResponse.StatusCode, errorBody);
                throw new HttpRequestException($"Gemini Imagen API error ({httpResponse.StatusCode}): {errorBody}");
            }

            var json = await httpResponse.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            // La respuesta de Imagen puede devolver base64 o URI según la configuración
            string? imageUrl = null;

            if (root.TryGetProperty("predictions", out var predictions) && predictions.GetArrayLength() > 0)
            {
                var first = predictions[0];

                // Caso 1: devuelve URI directa (Cloud Storage)
                if (first.TryGetProperty("gcsUri", out var gcsUri) && !string.IsNullOrEmpty(gcsUri.GetString()))
                {
                    imageUrl = gcsUri.GetString();
                }
                // Caso 2: devuelve base64 — guardar localmente y devolver URL relativa
                else if (first.TryGetProperty("bytesBase64Encoded", out var b64) && !string.IsNullOrEmpty(b64.GetString()))
                {
                    var bytes = Convert.FromBase64String(b64.GetString()!);
                    var fileName = $"gemini_{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}_{seed}.png";

                    // Asegurar directorio
                    if (!Directory.Exists(_uploadDir))
                        Directory.CreateDirectory(_uploadDir);

                    var filePath = Path.Combine(_uploadDir, fileName);
                    await File.WriteAllBytesAsync(filePath, bytes);

                    imageUrl = $"/uploads/generated/{fileName}";
                    _logger.LogInformation("Gemini Imagen: imagen guardada localmente en {FilePath}", filePath);
                }
            }

            if (string.IsNullOrWhiteSpace(imageUrl))
            {
                throw new InvalidOperationException("Gemini Imagen no devolvió una URL ni bytes de imagen.");
            }

            response.ImageUrl = imageUrl;
            response.Success = true;
            _logger.LogInformation("Gemini Imagen generó imagen exitosamente en {Ms}ms", stopwatch.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            response.Success = false;
            response.ErrorMessage = ex.Message;
            _logger.LogError(ex, "Error en GeminiImageProvider");
        }

        return response;
    }
}
