using System;
using System.Threading.Tasks;
using LuxuryCo.Database.Data;
using LuxuryCo.Database.Models;
using Microsoft.Extensions.Logging;

namespace LuxuryCo.Back.Services;

public class ImageMetadataService
{
    private readonly LuxuryCoDbContext _context;
    private readonly ILogger<ImageMetadataService> _logger;

    public ImageMetadataService(LuxuryCoDbContext context, ILogger<ImageMetadataService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task SaveMetadataAsync(
        string promptOriginal,
        string optimizedPrompt,
        string negativePrompt,
        int seed,
        string provider,
        double generationTimeMs,
        int? userId,
        string imageUrl)
    {
        try
        {
            var meta = new AiImageGeneration
            {
                PromptOriginal = promptOriginal,
                OptimizedPrompt = optimizedPrompt,
                NegativePrompt = negativePrompt,
                Seed = seed,
                Provider = provider,
                GenerationTimeMs = generationTimeMs,
                UserId = userId > 0 ? userId : null,
                ImageUrl = imageUrl,
                CreatedAt = DateTime.UtcNow
            };

            _context.AiImageGenerations.Add(meta);
            await _context.SaveChangesAsync();
            _logger.LogInformation($"Saved image generation metadata to database. ID: {meta.Id}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save image generation metadata to database.");
        }
    }
}
