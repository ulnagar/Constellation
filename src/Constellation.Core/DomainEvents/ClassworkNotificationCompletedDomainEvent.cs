namespace Constellation.Core.DomainEvents;

using Constellation.Core.Models.Identifiers;

public sealed record ClassworkNotificationCompletedDomainEvent(
    DomainEventId Id,
    ClassworkNotificationId NotificationId)
    : DomainEvent(Id);