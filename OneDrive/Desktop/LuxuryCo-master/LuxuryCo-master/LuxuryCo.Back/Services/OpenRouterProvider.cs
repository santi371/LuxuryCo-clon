using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Net.Http.Headers;

namespace LuxuryCo.Back.Services;

public class OpenRouterProvider : IAiProvider
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;
    private readonly ILogger<OpenRouterProvider> _logger;

    public string ProviderName => "OpenRouter";

    public OpenRouterProvider(HttpClient httpClient, IConfiguration config, ILogger<OpenRouterProvider> logger)
    {
        _httpClient = httpClient;
        _config = config;
        _logger = logger;
    }

    public async Task<ProviderResponse> GenerateCompletionAsync(string systemPrompt, string userPrompt, double temperature = 0.7)
    {
        var response = new ProviderResponse();
        var sw = Stopwatch.StartNew();

        try
        {
            // Read from config first, then env vars. Skip empty or placeholder values.
            var configKey = _config["OpenRouter:ApiKey"];
            var apiKey = (!string.IsNullOrWhiteSpace(configKey) ? configKey : null)
                ?? _config["openrouter:key"]
                ?? Environment.GetEnvironmentVariable("openrouter__key") 
                ?? Environment.GetEnvironmentVariable("open_router_key");
            
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                response.Success = false;
                response.ErrorMessage = $"OpenRouter API Key not configured. Config[openrouter:key]={_config["openrouter:key"] ?? "NULL"} | EnvVar={Environment.GetEnvironmentVariable("openrouter__key") ?? "NULL"}";
                return response;
            }
            apiKey = apiKey.Trim().Trim('"').Trim('\'').Replace("\n","").Replace("\r","");
            // Dynamically fetch available free models from OpenRouter, then try them in order
            var freeModels = await GetAvailableFreeModelsAsync(apiKey);

            string? lastError = null;
            foreach (var model in freeModels)
            {
                var requestBody = new
                {
                    model = model,
                    temperature = temperature,
                    messages = new[]
                    {
                        new { role = "system", content = systemPrompt },
                        new { role = "user", content = userPrompt }
                    }
                };

                var jsonContent = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

                var request = new HttpRequestMessage(HttpMethod.Post, "https://openrouter.ai/api/v1/chat/completions");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
                request.Headers.Add("HTTP-Referer", "https://luxuryco.com");
                request.Headers.Add("X-Title", "LuxuryCo AI");
                request.Content = jsonContent;

                var httpResponse = await _httpClient.SendAsync(request);
                var responseString = await httpResponse.Content.ReadAsStringAsync();

                sw.Stop();
                response.LatencyMs = sw.ElapsedMilliseconds;

                if (httpResponse.IsSuccessStatusCode)
                {
                    using var document = JsonDocument.Parse(responseString);
                    var root = document.RootElement;

                    var choices = root.GetProperty("choices");
                    if (choices.GetArrayLength() > 0)
                    {
                        var msg = choices[0].GetProperty("message");
                        response.Reply = msg.GetProperty("content").GetString() ?? "";
                        response.Success = true;
                        break; // Exit loop on success
                    }
                }

                var statusCode = (int)httpResponse.StatusCode;
                lastError = $"[{model}] {httpResponse.StatusCode}: {responseString}";

                // Continue to next model on 400 (invalid), 404 (unavailable), 429 (rate limited)
                if (statusCode != 400 && statusCode != 404 && statusCode != 429)
                {
                    _logger.LogWarning("OpenRouter fatal error on model {Model}: {Error}", model, lastError);
                    break;
                }

                _logger.LogWarning("OpenRouter model {Model} skipped ({Status}), trying next...", model, statusCode);
                sw = Stopwatch.StartNew();
            }

            if (!response.Success)
            {
                response.ErrorMessage = $"All OpenRouter models failed. Last error: {lastError}";
                _logger.LogWarning("All OpenRouter models failed: {Error}", response.ErrorMessage);
            }
        }
        catch (Exception ex)
        {
            sw.Stop();
            response.LatencyMs = sw.ElapsedMilliseconds;
            response.Success = false;
            response.ErrorMessage = $"Exception in OpenRouterProvider: {ex.Message}";
            _logger.LogError(ex, "Exception in OpenRouterProvider");
        }

        return response;
    }

    private async Task<IEnumerable<string>> GetAvailableFreeModelsAsync(string apiKey)
    {
        // Fallback list in case the API call fails
        var fallback = new[]
        {
            "mistralai/mistral-nemo:free",
            "deepseek/deepseek-chat-v3-0324:free",
            "google/gemma-2-9b-it:free",
            "qwen/qwen3-8b:free",
            "meta-llama/llama-3.2-3b-instruct:free",
            "microsoft/phi-3-mini-128k-instruct:free",
            "mistralai/mistral-7b-instruct:free"
        };

        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "https://openrouter.ai/api/v1/models");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

            var httpResponse = await _httpClient.SendAsync(request);
            if (!httpResponse.IsSuccessStatusCode)
                return fallback;

            var json = await httpResponse.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            var models = doc.RootElement.GetProperty("data");

            var freeModels = models.EnumerateArray()
                .Select(m => m.TryGetProperty("id", out var idProp) ? idProp.GetString() : null)
                .Where(id => id != null && id.EndsWith(":free"))
                .Select(id => id!)
                .OrderBy(id => id)
                .ToList();

            _logger.LogInformation("OpenRouter: found {Count} free models available.", freeModels.Count);
            return freeModels.Count > 0 ? freeModels : fallback;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to fetch OpenRouter model list, using fallback.");
            return fallback;
        }
    }
}
