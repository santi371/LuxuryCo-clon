using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace LuxuryCo.Back.Services;

public class StabilityProvider : IImageProvider
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;

    public string ProviderName => "Stability";

    public StabilityProvider(HttpClient httpClient, IConfiguration config)
    {
        _httpClient = httpClient;
        _apiKey = config["Stability:ApiKey"] ?? Environment.GetEnvironmentVariable("stability__key") ?? string.Empty;
    }

    public async Task<ImageGenerationProviderResponse> GenerateImageAsync(string optimizedPrompt, string negativePrompt, int seed)
    {
        if (string.IsNullOrWhiteSpace(_apiKey))
        {
            return new ImageGenerationProviderResponse
            {
                Success = false,
                ErrorMessage = "Stability API key is not configured."
            };
        }

        try
        {
            var requestBody = new
            {
                text_prompts = new[]
                {
                    new { text = optimizedPrompt, weight = 1.0 },
                    new { text = negativePrompt, weight = -1.0 }
                },
                cfg_scale = 7,
                height = 512,
                width = 512,
                samples = 1,
                steps = 30,
                seed = seed
            };

            using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.stability.ai/v1/generation/stable-diffusion-xl-1024-v1-0/text-to-image");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
            request.Content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                return new ImageGenerationProviderResponse
                {
                    Success = false,
                    ErrorMessage = $"Stability error: {error}"
                };
            }

            var content = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(content);
            var base64Image = doc.RootElement.GetProperty("artifacts")[0].GetProperty("base64").GetString() ?? string.Empty;

            // En lugar de una URL, retornamos el base64 convertido a una data URI, 
            // el router/storage service la convertirá en archivo físico.
            return new ImageGenerationProviderResponse
            {
                Success = true,
                ImageUrl = $"data:image/png;base64,{base64Image}"
            };
        }
        catch (Exception ex)
        {
            return new ImageGenerationProviderResponse
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }
}
