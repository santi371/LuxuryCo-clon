using System;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace LuxuryCo.Back.Services;

public class GeminiProvider : IAiProvider
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _model;

    public string ProviderName => "Gemini";

    public GeminiProvider(HttpClient httpClient, IConfiguration config)
    {
        _httpClient = httpClient;
        _apiKey = config["Gemini:ApiKey"] ?? Environment.GetEnvironmentVariable("gemini__key") ?? string.Empty;
        _model = config["Gemini:Model"] ?? "gemini-2.0-flash";
    }

    public async Task<ProviderResponse> GenerateCompletionAsync(string systemPrompt, string userPrompt, double temperature = 0.7)
    {
        var response = new ProviderResponse();
        var stopwatch = Stopwatch.StartNew();

        try
        {
            if (string.IsNullOrWhiteSpace(_apiKey))
            {
                throw new InvalidOperationException("Gemini API Key is not configured.");
            }

            var requestBody = new
            {
                systemInstruction = new
                {
                    parts = new[] { new { text = systemPrompt } }
                },
                contents = new[]
                {
                    new
                    {
                        role = "user",
                        parts = new[] { new { text = userPrompt } }
                    }
                },
                generationConfig = new
                {
                    temperature = temperature
                }
            };

            var url = $"https://generativelanguage.googleapis.com/v1beta/models/{_model}:generateContent?key={_apiKey}";
            using var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

            var httpResponse = await _httpClient.SendAsync(request);
            stopwatch.Stop();
            response.LatencyMs = stopwatch.Elapsed.TotalMilliseconds;

            if (!httpResponse.IsSuccessStatusCode)
            {
                var errorContent = await httpResponse.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Gemini API error (status {httpResponse.StatusCode}): {errorContent}");
            }

            var json = await httpResponse.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            // Extract reply
            response.Reply = root.GetProperty("candidates")[0]
                                 .GetProperty("content")
                                 .GetProperty("parts")[0]
                                 .GetProperty("text")
                                 .GetString() ?? string.Empty;

            // Estimate tokens (approximate representation if not in payload metadata)
            if (root.TryGetProperty("usageMetadata", out var usage))
            {
                response.PromptTokens = usage.TryGetProperty("promptTokenCount", out var pt) ? pt.GetInt32() : 0;
                response.CompletionTokens = usage.TryGetProperty("candidatesTokenCount", out var ct) ? ct.GetInt32() : 0;
                
                // Estimated cost: Gemini 1.5 Flash cost: ~$0.075/1M prompt tokens, ~$0.30/1M completion tokens
                response.EstimatedCostUsd = ((response.PromptTokens * 0.075) + (response.CompletionTokens * 0.30)) / 1000000.0;
            }
            else
            {
                // Raw estimate fallback
                response.PromptTokens = (systemPrompt.Length + userPrompt.Length) / 4;
                response.CompletionTokens = response.Reply.Length / 4;
                response.EstimatedCostUsd = ((response.PromptTokens * 0.075) + (response.CompletionTokens * 0.30)) / 1000000.0;
            }

            response.Success = true;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            response.LatencyMs = stopwatch.Elapsed.TotalMilliseconds;
            response.Success = false;
            response.ErrorMessage = ex.Message;
            response.Reply = string.Empty;
        }

        return response;
    }
}
