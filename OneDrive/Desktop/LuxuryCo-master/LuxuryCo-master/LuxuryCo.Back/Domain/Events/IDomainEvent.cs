using MediatR;
using System;

namespace LuxuryCo.Back.Domain.Events;

public interface IDomainEvent : INotification
{
    Guid EventId { get; }
    DateTime OccurredOn { get; }
}
