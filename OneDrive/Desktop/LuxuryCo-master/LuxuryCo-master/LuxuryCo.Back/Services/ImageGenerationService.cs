using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace LuxuryCo.Back.Services;

public class ImageGenerationService
{
    private readonly ImagePromptOptimizerService _optimizer;
    private readonly ImageModerationService _moderation;
    private readonly ImageCacheService _cache;
    private readonly ImageStorageService _storage;
    private readonly ImageProviderRouter _router;
    private readonly ImageMetadataService _metadata;
    private readonly ILogger<ImageGenerationService> _logger;

    public ImageGenerationService(
        ImagePromptOptimizerService optimizer,
        ImageModerationService moderation,
        ImageCacheService cache,
        ImageStorageService storage,
        ImageProviderRouter router,
        ImageMetadataService metadata,
        ILogger<ImageGenerationService> logger)
    {
        _optimizer = optimizer;
        _moderation = moderation;
        _cache = cache;
        _storage = storage;
        _router = router;
        _metadata = metadata;
        _logger = logger;
    }

    public async Task<ImageGenerationResultDto> GenerateImageAsync(string originalPrompt, int? userId, int? customSeed = null)
    {
        var stopwatch = Stopwatch.StartNew();
        var result = new ImageGenerationResultDto { Prompt = originalPrompt };

        int activeUserId = userId ?? 0;

        // 1. Quotas and Limits check
        if (!_cache.CheckQuota(activeUserId))
        {
            result.Status = "QuotaExceeded";
            result.Prompt = "Has excedido el límite diario de generaciones de imágenes de lujo.";
            return result;
        }

        // 2. Cooldown check
        if (!_cache.CheckCooldown(activeUserId, out int secondsLeft))
        {
            result.Status = "Cooldown";
            result.Prompt = $"Por favor, espera {secondsLeft} segundos antes de generar otro diseño.";
            return result;
        }

        // 3. Prompt Moderation
        if (!_moderation.IsPromptSafe(originalPrompt, out string violationReason))
        {
            result.Status = "Blocked";
            result.Prompt = violationReason;
            return result;
        }

        // 4. Duplicate / Cache Check
        if (customSeed == null && _cache.TryGetCachedImage(originalPrompt, out string cachedUrl))
        {
            stopwatch.Stop();
            result.ImageUrl = cachedUrl;
            result.Seed = 12345; // Default seed for cached
            result.Status = "Success";
            result.Provider = "Cache";
            result.GenerationTimeMs = stopwatch.Elapsed.TotalMilliseconds;
            return result;
        }

        try
        {
            // Set Cooldown
            _cache.SetCooldown(activeUserId);

            // 5. Optimize Prompt
            var optimizedPrompt = await _optimizer.OptimizePromptAsync(originalPrompt);
            var negativePrompt = _optimizer.GetNegativePrompt();

            // 6. Seed selection (Seed rotation)
            int seed = customSeed ?? new Random().Next(1, int.MaxValue);
            result.Seed = seed;

            // 7. Route and execute generation (Polly retries)
            var response = await _router.GenerateImageWithFallbackAsync(optimizedPrompt, negativePrompt, seed);
            if (!response.Success)
            {
                result.Status = "Failed";
                result.Prompt = $"Error en los proveedores de generación: {response.ErrorMessage}";
                return result;
            }

            // 8. Secure Storage (Supabase CDN fallback to local wwwroot)
            var storedUrl = await _storage.StoreImageAsync(response.ImageUrl);

            stopwatch.Stop();

            // 9. Persist Metadata
            await _metadata.SaveMetadataAsync(
                originalPrompt,
                optimizedPrompt,
                negativePrompt,
                seed,
                response.ImageUrl.Contains("stability.ai") ? "Stability" : "Pollinations",
                stopwatch.Elapsed.TotalMilliseconds,
                activeUserId,
                storedUrl
            );

            // 10. Cache image URL
            if (customSeed == null)
            {
                _cache.CacheImage(originalPrompt, storedUrl);
            }

            // Increment daily usage
            _cache.IncrementQuota(activeUserId);

            // 11. Populate DTO
            result.ImageUrl = storedUrl;
            result.Status = "Success";
            result.Provider = response.ImageUrl.Contains("stability.ai") ? "Stability" : "Pollinations";
            result.GenerationTimeMs = stopwatch.Elapsed.TotalMilliseconds;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Failed to generate luxury image.");
            result.Status = "Failed";
            result.Prompt = $"Error al procesar la generación: {ex.Message}";
        }

        return result;
    }
}

public class ImageGenerationResultDto
{
    public string Type { get; set; } = "image";
    public string ImageUrl { get; set; } = string.Empty;
    public string Prompt { get; set; } = string.Empty;
    public int Seed { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty;
    public double GenerationTimeMs { get; set; }
}
