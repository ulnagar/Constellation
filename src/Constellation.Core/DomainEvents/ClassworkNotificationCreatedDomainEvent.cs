namespace Constellation.Core.DomainEvents;

using Constellation.Core.Models.Identifiers;

public sealed record ClassworkNotificationCreatedDomainEvent(
    DomainEventId Id,
    ClassworkNotificationId NotificationId)
    : DomainEvent(Id);
