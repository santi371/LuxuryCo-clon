using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace LuxuryCo.Back.Services;

public class ConfirmationService
{
    private static readonly ConcurrentDictionary<string, PendingAction> PendingActions = new();

    public Task<string> RegisterPendingActionAsync(int userId, string intent, object parameters, string description)
    {
        var token = Guid.NewGuid().ToString("N");
        var pending = new PendingAction
        {
            Token = token,
            UserId = userId,
            Intent = intent,
            Parameters = parameters,
            Description = description,
            CreatedAt = DateTime.UtcNow
        };

        PendingActions[token] = pending;
        return Task.FromResult(token);
    }

    public Task<PendingAction?> GetPendingActionAsync(string token)
    {
        PendingActions.TryGetValue(token, out var action);
        return Task.FromResult(action);
    }

    public Task<bool> CompleteActionAsync(string token)
    {
        return Task.FromResult(PendingActions.TryRemove(token, out _));
    }
}

public class PendingAction
{
    public string Token { get; set; } = string.Empty;
    public int UserId { get; set; }
    public string Intent { get; set; } = string.Empty;
    public object Parameters { get; set; } = new();
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
