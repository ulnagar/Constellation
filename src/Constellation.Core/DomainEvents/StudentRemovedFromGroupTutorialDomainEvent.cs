namespace Constellation.Core.DomainEvents;

using Constellation.Core.Models.Identifiers;

public sealed record StudentRemovedFromGroupTutorialDomainEvent(
    DomainEventId Id,
    GroupTutorialId TutorialId,
    TutorialEnrolmentId EnrolmentId)
    : DomainEvent(Id);