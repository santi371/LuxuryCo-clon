using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Polly;

namespace LuxuryCo.Back.Services;

/// <summary>
/// Enruta la generación de imágenes entre proveedores con fallback automático:
/// Pollinations → Stability → Gemini Imagen
/// Usa Polly para reintentos resilientes en cada proveedor.
/// </summary>
public class ImageProviderRouter
{
    private readonly PollinationsProvider _pollinations;
    private readonly StabilityProvider _stability;
    private readonly GeminiImageProvider _geminiImage;
    private readonly ILogger<ImageProviderRouter> _logger;

    // Retry policy: 2 reintentos con backoff exponencial
    private readonly AsyncPolicy _resiliencePolicy;

    public ImageProviderRouter(
        PollinationsProvider pollinations,
        StabilityProvider stability,
        GeminiImageProvider geminiImage,
        ILogger<ImageProviderRouter> logger)
    {
        _pollinations = pollinations;
        _stability = stability;
        _geminiImage = geminiImage;
        _logger = logger;

        _resiliencePolicy = Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(2, retryAttempt => TimeSpan.FromMilliseconds(200 * retryAttempt));
    }

    public async Task<ImageGenerationProviderResponse> GenerateImageWithFallbackAsync(
        string optimizedPrompt,
        string negativePrompt,
        int seed)
    {
        // ─────────────────────────────────────────────────────────────────
        // 1. Intento: Pollinations (gratuito, sin clave)
        // ─────────────────────────────────────────────────────────────────
        try
        {
            _logger.LogInformation("[ImageRouter] Intentando Pollinations...");
            var res = await _resiliencePolicy.ExecuteAsync(() =>
                _pollinations.GenerateImageAsync(optimizedPrompt, negativePrompt, seed));

            if (res.Success)
            {
                _logger.LogInformation("[ImageRouter] ✅ Pollinations exitoso.");
                res.ErrorMessage = string.Empty;
                return res;
            }
            _logger.LogWarning("[ImageRouter] ⚠️ Pollinations falló: {Error}", res.ErrorMessage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[ImageRouter] Excepción en Pollinations.");
        }

        // ─────────────────────────────────────────────────────────────────
        // 2. Fallback: Stability AI (requiere STABILITY_API_KEY)
        // ─────────────────────────────────────────────────────────────────
        try
        {
            _logger.LogInformation("[ImageRouter] Intentando Stability AI...");
            var res = await _resiliencePolicy.ExecuteAsync(() =>
                _stability.GenerateImageAsync(optimizedPrompt, negativePrompt, seed));

            if (res.Success)
            {
                _logger.LogInformation("[ImageRouter] ✅ Stability AI exitoso.");
                return res;
            }
            _logger.LogWarning("[ImageRouter] ⚠️ Stability AI falló: {Error}", res.ErrorMessage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[ImageRouter] Excepción en Stability AI.");
        }

        // ─────────────────────────────────────────────────────────────────
        // 3. Fallback: Gemini Imagen (requiere Gemini:ApiKey en appsettings)
        // ─────────────────────────────────────────────────────────────────
        try
        {
            _logger.LogInformation("[ImageRouter] Intentando Gemini Imagen...");
            var altSeed = seed + 100;
            var res = await _resiliencePolicy.ExecuteAsync(() =>
                _geminiImage.GenerateImageAsync(optimizedPrompt, negativePrompt, altSeed));

            if (res.Success)
            {
                _logger.LogInformation("[ImageRouter] ✅ Gemini Imagen exitoso.");
                return res;
            }
            _logger.LogWarning("[ImageRouter] ⚠️ Gemini Imagen falló: {Error}", res.ErrorMessage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[ImageRouter] Todos los proveedores de imagen han fallado.");
        }

        // ─────────────────────────────────────────────────────────────────
        // Todos los proveedores han fallado
        // ─────────────────────────────────────────────────────────────────
        _logger.LogCritical("[ImageRouter] ❌ Todos los proveedores de generación de imágenes fallaron para el prompt: {Prompt}", optimizedPrompt);
        return new ImageGenerationProviderResponse
        {
            Success = false,
            ErrorMessage = "Todos los proveedores de imágenes están temporalmente no disponibles. Por favor, intenta de nuevo en unos minutos."
        };
    }
}
