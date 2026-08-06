using System;

namespace LuxuryCo.Back.Domain.Events;

public class ReportGeneratedEvent : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;

    public int AdminId { get; }
    public string ReportType { get; }
    public string SecureUrl { get; }

    public ReportGeneratedEvent(int adminId, string reportType, string secureUrl)
    {
        AdminId = adminId;
        ReportType = reportType;
        SecureUrl = secureUrl;
    }
}
