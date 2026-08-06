using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace LuxuryCo.Back.Hubs;

// Restrict to authenticated admins
[Authorize(Roles = "ADMIN")]
public class AdminNotificationHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        // Join a specific group for admin notifications
        await Groups.AddToGroupAsync(Context.ConnectionId, "AdminsGroup");
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(System.Exception? exception)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, "AdminsGroup");
        await base.OnDisconnectedAsync(exception);
    }
}
