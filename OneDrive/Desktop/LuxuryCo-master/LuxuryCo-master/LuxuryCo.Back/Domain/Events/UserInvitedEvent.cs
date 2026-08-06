using System;

namespace LuxuryCo.Back.Domain.Events;

public class UserInvitedEvent : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;

    public int AdminId { get; }
    public string Name { get; }
    public string Email { get; }
    public string InviteToken { get; }

    public UserInvitedEvent(int adminId, string name, string email, string inviteToken)
    {
        AdminId = adminId;
        Name = name;
        Email = email;
        InviteToken = inviteToken;
    }
}
