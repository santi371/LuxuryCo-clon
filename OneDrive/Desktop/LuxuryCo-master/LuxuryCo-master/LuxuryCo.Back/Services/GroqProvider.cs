using System;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace LuxuryCo.Back.Services;

public class GroqProvider : IAiProvider
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _model;
    private static readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    public string ProviderName => "Groq";

    public GroqProvider(HttpClient httpClient, IConfiguration config)
    {
        _httpClient = httpClient;
        _apiKey = config["Groq:ApiKey"] ?? string.Empty;
        _model = config["Groq:Model"] ?? "llama-3.1-8b-instant";
    }

    public async Task<ProviderResponse> GenerateCompletionAsync(string systemPrompt, string userPrompt, double temperature = 0.7)
    {
        var response = new ProviderResponse();
        var stopwatch = Stopwatch.StartNew();

        try
        {
            if (string.IsNullOrWhiteSpace(_apiKey))
            {
                throw new InvalidOperationException("Groq API Key is not configured.");
            }

            var requestBody = new
            {
                model = _model,
                messages = new[]
                {
                    new { role = "system", content = systemPrompt },
                    new { role = "user", content = userPrompt }
                },
                temperature = temperature
            };

            using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.groq.com/openai/v1/chat/completions");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
            request.Content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

            var httpResponse = await _httpClient.SendAsync(request);
            stopwatch.Stop();
            response.LatencyMs = stopwatch.Elapsed.TotalMilliseconds;

            if (!httpResponse.IsSuccessStatusCode)
            {
                var errorContent = await httpResponse.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Groq API error (status {httpResponse.StatusCode}): {errorContent}");
            }

            var json = await httpResponse.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            response.Reply = root.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString() ?? string.Empty;
            
            // Extract usage metrics if available
            if (root.TryGetProperty("usage", out var usage))
            {
                response.PromptTokens = usage.TryGetProperty("prompt_tokens", out var pt) ? pt.GetInt32() : 0;
                response.CompletionTokens = usage.TryGetProperty("completion_tokens", out var ct) ? ct.GetInt32() : 0;
                
                // Estimated cost: Llama 3.1 8B cost: ~$0.05/1M prompt tokens, ~$0.08/1M completion tokens
                response.EstimatedCostUsd = ((response.PromptTokens * 0.05) + (response.CompletionTokens * 0.08)) / 1000000.0;
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
