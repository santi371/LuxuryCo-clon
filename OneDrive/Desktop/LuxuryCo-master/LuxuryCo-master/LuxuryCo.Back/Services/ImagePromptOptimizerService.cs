using System;
using System.Threading.Tasks;

namespace LuxuryCo.Back.Services;

public class ImagePromptOptimizerService
{
    private readonly GroqProvider _groqProvider;

    public ImagePromptOptimizerService(GroqProvider groqProvider)
    {
        _groqProvider = groqProvider;
    }

    public async Task<string> OptimizePromptAsync(string originalPrompt)
    {
        if (string.IsNullOrWhiteSpace(originalPrompt))
            return "A luxury high-end fashion garment design, editorial photography, 8k resolution, photorealistic.";

        // Traducir y optimizar para Luxury Fashion usando Groq
        var systemInstruction = @"You are a Luxury Fashion Prompt Optimizer. 
Translate the user input to English if it is in Spanish or another language, and enhance it to be a professional, photorealistic luxury fashion image generation prompt. 
Include key terms like: 'luxury brand editorial photography', 'high-end premium fabric textures', 'soft studio lighting', 'haute couture details', 'highly detailed 8k resolution'. 
Do not include conversational filler, markdown formatting, or introductory text. ONLY output the optimized English prompt.";

        try
        {
            var response = await _groqProvider.GenerateCompletionAsync(systemInstruction, originalPrompt, temperature: 0.3);
            if (response.Success && !string.IsNullOrWhiteSpace(response.Reply))
            {
                return response.Reply.Trim();
            }
        }
        catch
        {
            // Fallback rules if Groq fails
        }

        // Basic heuristic fallback if AI translation fails
        return $"Premium luxury high fashion garment design, {originalPrompt}, luxury brand editorial photography, high-end premium fabric textures, soft studio lighting, highly detailed 8k resolution";
    }

    public string GetNegativePrompt()
    {
        return "ugly, deformed, poor quality, bad anatomy, bad lighting, low resolution, cheap fabric, distorted, blurry, watermark, signature, draft, text, brand labels";
    }
}
