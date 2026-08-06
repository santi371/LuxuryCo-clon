using System.Threading.Tasks;

namespace LuxuryCo.Back.Services;

public interface IAiProvider
{
    string ProviderName { get; }
    Task<ProviderResponse> GenerateCompletionAsync(string systemPrompt, string userPrompt, double temperature = 0.7);
}

public class ProviderResponse
{
    public string Reply { get; set; } = string.Empty;
    public int PromptTokens { get; set; }
    public int CompletionTokens { get; set; }
    public double EstimatedCostUsd { get; set; }
    public double LatencyMs { get; set; }
    public bool Success { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
}
