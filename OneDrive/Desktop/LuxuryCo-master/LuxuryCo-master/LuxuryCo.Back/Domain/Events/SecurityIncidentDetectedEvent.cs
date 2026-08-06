using System;

namespace LuxuryCo.Back.Domain.Events;

public class SecurityIncidentDetectedEvent : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;

    public string IncidentType { get; }
    public string Description { get; }
    public string IpAddress { get; }
    public int? AdminId { get; }

    public SecurityIncidentDetectedEvent(string incidentType, string description, string ipAddress, int? adminId)
    {
        IncidentType = incidentType;
        Description = description;
        IpAddress = ipAddress;
        AdminId = adminId;
    }
}
