using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace LuxuryCo.Back.Services;

public class WhisperProvider
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;

    public WhisperProvider(HttpClient httpClient, IConfiguration config)
    {
        _httpClient = httpClient;
        _apiKey = config["Groq:ApiKey"] ?? string.Empty;
    }

    public async Task<string> TranscribeAudioAsync(byte[] audioBytes, string filename = "voice.wav")
    {
        if (string.IsNullOrWhiteSpace(_apiKey))
        {
            throw new InvalidOperationException("Groq API Key (Whisper) is not configured.");
        }

        using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.groq.com/openai/v1/audio/transcriptions");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

        using var content = new MultipartFormDataContent();
        
        var fileContent = new ByteArrayContent(audioBytes);
        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("audio/wav");
        content.Add(fileContent, "file", filename);

        content.Add(new StringContent("whisper-large-v3"), "model");
        content.Add(new StringContent("es"), "language");

        request.Content = content;

        var httpResponse = await _httpClient.SendAsync(request);
        if (!httpResponse.IsSuccessStatusCode)
        {
            var errorContent = await httpResponse.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Whisper transcription failed: {errorContent}");
        }

        var json = await httpResponse.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        return doc.RootElement.GetProperty("text").GetString() ?? string.Empty;
    }
}
