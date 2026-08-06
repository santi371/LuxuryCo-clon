using System;
using System.Collections.Generic;
using System.Linq;

namespace LuxuryCo.Back.Services;

public class ImageModerationService
{
    private static readonly HashSet<string> BannedKeywords = new(StringComparer.OrdinalIgnoreCase)
    {
        "nsfw", "naked", "nude", "porn", "sex", "gore", "violence", "blood", "kill", "die", "weapons", "drugs"
    };

    private static readonly HashSet<string> AllowedDomains = new(StringComparer.OrdinalIgnoreCase)
    {
        "image.pollinations.ai",
        "api.stability.ai",
        "generativelanguage.googleapis.com",
        "luxuryco.onrender.com",
        "localhost"
    };

    public bool IsPromptSafe(string prompt, out string reason)
    {
        if (string.IsNullOrWhiteSpace(prompt))
        {
            reason = "El prompt no puede estar vacío.";
            return false;
        }

        var tokens = prompt.Split(new[] { ' ', ',', '.', ';', '-', '_' }, StringSplitOptions.RemoveEmptyEntries);
        foreach (var token in tokens)
        {
            if (BannedKeywords.Contains(token))
            {
                reason = $"El prompt contiene palabras no permitidas por políticas de seguridad de IA: '{token}'.";
                return false;
            }
        }

        reason = string.Empty;
        return true;
    }

    public bool IsUrlAllowed(string url)
    {
        if (string.IsNullOrWhiteSpace(url)) return false;

        try
        {
            var uri = new Uri(url);
            var host = uri.Host;
            return AllowedDomains.Any(domain => host.Equals(domain, StringComparison.OrdinalIgnoreCase) || host.EndsWith("." + domain, StringComparison.OrdinalIgnoreCase));
        }
        catch
        {
            return false;
        }
    }
}
