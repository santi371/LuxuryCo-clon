using System;
using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;

namespace LuxuryCo.Back.Services;

public class ImageCacheService
{
    private readonly ConcurrentDictionary<string, string> _promptCache = new();
    private readonly ConcurrentDictionary<int, DateTime> _userCooldowns = new();
    private readonly ConcurrentDictionary<int, int> _userDailyQuota = new();

    public string NormalizePrompt(string prompt)
    {
        if (string.IsNullOrWhiteSpace(prompt)) return string.Empty;
        return prompt.Trim().ToLowerInvariant().Replace("  ", " ");
    }

    public string ComputePromptHash(string prompt)
    {
        var normalized = NormalizePrompt(prompt);
        using var sha = SHA256.Create();
        var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(normalized));
        return Convert.ToHexString(bytes);
    }

    public bool TryGetCachedImage(string prompt, out string imageUrl)
    {
        var hash = ComputePromptHash(prompt);
        return _promptCache.TryGetValue(hash, out imageUrl!);
    }

    public void CacheImage(string prompt, string imageUrl)
    {
        var hash = ComputePromptHash(prompt);
        _promptCache[hash] = imageUrl;
    }

    public bool CheckCooldown(int userId, out int secondsLeft)
    {
        secondsLeft = 0;
        if (userId == 0) return true; // No cooldown for visitors or default handling

        if (_userCooldowns.TryGetValue(userId, out var lastGen))
        {
            var elapsed = DateTime.UtcNow - lastGen;
            if (elapsed.TotalSeconds < 15) // 15-second cooldown
            {
                secondsLeft = (int)(15 - elapsed.TotalSeconds);
                return false;
            }
        }
        return true;
    }

    public void SetCooldown(int userId)
    {
        if (userId > 0)
        {
            _userCooldowns[userId] = DateTime.UtcNow;
        }
    }

    public bool CheckQuota(int userId, int limit = 20)
    {
        if (userId == 0) return true;
        
        var count = _userDailyQuota.GetOrAdd(userId, 0);
        return count < limit;
    }

    public void IncrementQuota(int userId)
    {
        if (userId > 0)
        {
            _userDailyQuota.AddOrUpdate(userId, 1, (key, oldVal) => oldVal + 1);
        }
    }
}
