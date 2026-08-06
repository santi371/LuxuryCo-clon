using System;
using System.Threading.Tasks;

namespace LuxuryCo.Back.Services;

public class PollinationsProvider : IImageProvider
{
    public string ProviderName => "Pollinations";

    public PollinationsProvider() { }

    public Task<ImageGenerationProviderResponse> GenerateImageAsync(string optimizedPrompt, string negativePrompt, int seed)
    {
        try
        {
            // Pollinations genera imágenes via URL directamente - no se necesita verificar el endpoint
            // La imagen se carga en el navegador del cliente, no en el servidor
            var promptCombined = optimizedPrompt;
            if (!string.IsNullOrWhiteSpace(negativePrompt))
            {
                promptCombined += $", avoid: {negativePrompt}";
            }
            
            var encodedPrompt = Uri.EscapeDataString(promptCombined);
            var url = $"https://image.pollinations.ai/prompt/{encodedPrompt}?width=768&height=768&seed={seed}&nologo=true&model=flux";

            return Task.FromResult(new ImageGenerationProviderResponse
            {
                Success = true,
                ImageUrl = url
            });
        }
        catch (Exception ex)
        {
            return Task.FromResult(new ImageGenerationProviderResponse
            {
                Success = false,
                ErrorMessage = ex.Message
            });
        }
    }
}
