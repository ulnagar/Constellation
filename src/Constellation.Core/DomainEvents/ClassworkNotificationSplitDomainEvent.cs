namespace Constellation.Core.DomainEvents;

using Constellation.Core.Models.Identifiers;

public sealed record ClassworkNotificationSplitDomainEvent(
    DomainEventId Id,
    ClassworkNotificationId NotificationId)
    : DomainEvent(Id);