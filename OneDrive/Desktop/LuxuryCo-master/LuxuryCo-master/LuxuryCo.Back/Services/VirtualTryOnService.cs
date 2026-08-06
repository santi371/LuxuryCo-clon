using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace LuxuryCo.Back.Services;

/// <summary>
/// Probador Virtual Real con Fashn.ai
/// Flujo: usuario sube foto real -> Fashn.ai coloca la prenda del catalogo sobre ella
/// Fallback: si no hay API key de Fashn, usa Gemini Vision + Pollinations (estimacion visual)
/// </summary>
public class VirtualTryOnService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;
    private readonly ILogger<VirtualTryOnService> _logger;

    private const string FashnApiBase = "https://api.fashn.ai/v1";
    private const int MaxPollAttempts = 20;        // 20 x 3seg = hasta 60 segundos
    private const int PollIntervalMs   = 3000;

    public VirtualTryOnService(HttpClient httpClient, IConfiguration config, ILogger<VirtualTryOnService> logger)
    {
        _httpClient = httpClient;
        _config     = config;
        _logger     = logger;
    }

    public async Task<VirtualTryOnResultDto> TryOnAsync(
        string userPhotoBase64,
        string userPhotoMimeType,
        string garmentDescription,
        string? garmentImageUrl,
        string category  = "tops",
        int    seed      = 0)
    {
        var result = new VirtualTryOnResultDto();
        var sw     = System.Diagnostics.Stopwatch.StartNew();

        var fashnKey = _config["Fashn:ApiKey"] ?? _config["Fashn__ApiKey"] ?? string.Empty;

        // ── RUTA A: Fashn.ai (proba virtual real) ────────────────────────────
        if (!string.IsNullOrWhiteSpace(fashnKey) && !string.IsNullOrWhiteSpace(garmentImageUrl))
        {
            try
            {
                _logger.LogInformation("[VirtualTryOn] Usando Fashn.ai para prueba virtual real...");
                var fashnResult = await TryOnWithFashnAsync(fashnKey, userPhotoBase64, userPhotoMimeType, garmentImageUrl, category);
                if (fashnResult != null)
                {
                    sw.Stop();
                    result.ImageUrl        = fashnResult;
                    result.Provider        = "Fashn.ai";
                    result.Status          = "Success";
                    result.GenerationTimeMs = sw.Elapsed.TotalMilliseconds;
                    return result;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[VirtualTryOn] Fashn.ai fallo. Usando Pollinations como fallback.");
            }
        }

        // ── RUTA B: Gemini Vision + Pollinations (estimacion visual) ─────────
        _logger.LogInformation("[VirtualTryOn] Usando Gemini + Pollinations (visualizacion estimada)...");
        var personDesc = await AnalyzePersonWithGeminiAsync(userPhotoBase64, userPhotoMimeType);
        var prompt     = BuildFashionPrompt(personDesc, garmentDescription);
        int actualSeed = seed > 0 ? seed : new Random().Next(1, int.MaxValue);
        var encoded    = Uri.EscapeDataString(prompt);
        var imageUrl   = $"https://image.pollinations.ai/prompt/{encoded}?width=768&height=1024&seed={actualSeed}&nologo=true&model=flux";

        sw.Stop();
        result.ImageUrl        = imageUrl;
        result.Prompt          = prompt;
        result.PersonSummary   = personDesc;
        result.Seed            = actualSeed;
        result.Provider        = "Pollinations (estimacion)";
        result.Status          = "Success";
        result.GenerationTimeMs = sw.Elapsed.TotalMilliseconds;
        return result;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Fashn.ai: prueba virtual real — la prenda se superpone en la foto real
    // ─────────────────────────────────────────────────────────────────────────
    private async Task<string?> TryOnWithFashnAsync(
        string apiKey,
        string base64Photo,
        string mimeType,
        string garmentUrl,
        string category)
    {
        // Armar Data URI de la foto del usuario
        var safeMime  = string.IsNullOrWhiteSpace(mimeType) ? "image/jpeg" : mimeType;
        var dataUri   = $"data:{safeMime};base64,{base64Photo}";

        // Mapear categoria al formato de Fashn
        var fashnCategory = category.ToLower() switch
        {
            "bottoms" or "pantalon" or "falda" or "shorts" => "bottoms",
            "one-pieces" or "vestido" or "dress"           => "one-pieces",
            _                                               => "tops"      // default: tops
        };

        // Paso 1: Iniciar prediccion
        var runPayload = new
        {
            model_image   = dataUri,
            garment_image = garmentUrl,
            category      = fashnCategory,
            mode          = "quality"
        };

        var request = new HttpRequestMessage(HttpMethod.Post, $"{FashnApiBase}/run")
        {
            Content = new StringContent(JsonSerializer.Serialize(runPayload), Encoding.UTF8, "application/json")
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

        var runResponse = await _httpClient.SendAsync(request);
        if (!runResponse.IsSuccessStatusCode)
        {
            var err = await runResponse.Content.ReadAsStringAsync();
            _logger.LogWarning("[VirtualTryOn] Fashn.ai /run fallo {Status}: {Err}", runResponse.StatusCode, err);
            return null;
        }

        var runJson   = await runResponse.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(runJson);
        var predId    = doc.RootElement.GetProperty("id").GetString();
        if (string.IsNullOrEmpty(predId)) return null;

        _logger.LogInformation("[VirtualTryOn] Fashn prediccion iniciada: {Id}", predId);

        // Paso 2: Polling hasta obtener resultado
        for (int i = 0; i < MaxPollAttempts; i++)
        {
            await Task.Delay(PollIntervalMs);

            var statusReq = new HttpRequestMessage(HttpMethod.Get, $"{FashnApiBase}/status/{predId}");
            statusReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

            var statusResp = await _httpClient.SendAsync(statusReq);
            if (!statusResp.IsSuccessStatusCode) continue;

            var statusJson = await statusResp.Content.ReadAsStringAsync();
            using var statusDoc = JsonDocument.Parse(statusJson);
            var status = statusDoc.RootElement.GetProperty("status").GetString();

            _logger.LogInformation("[VirtualTryOn] Estado Fashn [{Attempt}]: {Status}", i + 1, status);

            if (status == "completed")
            {
                var output = statusDoc.RootElement.GetProperty("output");
                if (output.ValueKind == JsonValueKind.Array && output.GetArrayLength() > 0)
                {
                    return output[0].GetString();
                }
            }

            if (status == "failed" || status == "cancelled")
            {
                _logger.LogWarning("[VirtualTryOn] Fashn fallo con estado: {Status}", status);
                return null;
            }
        }

        _logger.LogWarning("[VirtualTryOn] Fashn timeout despues de {Attempts} intentos.", MaxPollAttempts);
        return null;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Gemini Vision: describe la persona de la foto para Pollinations
    // ─────────────────────────────────────────────────────────────────────────
    private async Task<string> AnalyzePersonWithGeminiAsync(string base64Image, string mimeType)
    {
        var apiKey = _config["Gemini:ApiKey"] ?? _config["Gemini__ApiKey"] ?? string.Empty;
        if (string.IsNullOrWhiteSpace(apiKey))
            return "a stylish person with a confident posture, standing in a fashion studio";

        var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent?key={apiKey}";

        var body = new
        {
            contents = new[]
            {
                new
                {
                    parts = new object[]
                    {
                        new { text = "You are a fashion photography expert. Describe this person in ONE short English sentence for image generation: body type, apparent gender, skin tone, posture, hair color/length. Do NOT mention their current clothes. Example: 'a slender young woman with long brown hair, olive skin, standing upright facing forward'." },
                        new { inline_data = new { mime_type = string.IsNullOrWhiteSpace(mimeType) ? "image/jpeg" : mimeType, data = base64Image } }
                    }
                }
            },
            generationConfig = new { temperature = 0.2, maxOutputTokens = 120 }
        };

        var response = await _httpClient.PostAsync(url,
            new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json"));

        if (!response.IsSuccessStatusCode)
            return "a stylish person with a confident posture, standing in a fashion studio";

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        return doc.RootElement
            .GetProperty("candidates")[0]
            .GetProperty("content")
            .GetProperty("parts")[0]
            .GetProperty("text")
            .GetString()?.Trim()
            ?? "a stylish person standing in a fashion studio";
    }

    private static string BuildFashionPrompt(string personDescription, string garmentDescription)
        => $"Professional luxury fashion editorial photo of {personDescription} " +
           $"wearing {garmentDescription}, " +
           "haute couture runway style, fashion magazine editorial, " +
           "studio lighting, pure white background, high-end fashion photography, " +
           "sharp focus, 8K ultra-detailed, professional model pose, Vogue magazine quality";
}

// DTOs
public class VirtualTryOnResultDto
{
    public string ImageUrl        { get; set; } = string.Empty;
    public string Prompt          { get; set; } = string.Empty;
    public string PersonSummary   { get; set; } = string.Empty;
    public int    Seed            { get; set; }
    public string Status          { get; set; } = string.Empty;
    public string Provider        { get; set; } = string.Empty;
    public string ErrorMessage    { get; set; } = string.Empty;
    public double GenerationTimeMs { get; set; }
    public bool   IsEstimation    => Provider?.Contains("Pollinations") == true;
}
