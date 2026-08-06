using System.Threading.Tasks;

namespace LuxuryCo.Back.Services;

public interface IImageProvider
{
    string ProviderName { get; }
    Task<ImageGenerationProviderResponse> GenerateImageAsync(string optimizedPrompt, string negativePrompt, int seed);
}

public class ImageGenerationProviderResponse
{
    public string ImageUrl { get; set; } = string.Empty;
    public bool Success { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
}
