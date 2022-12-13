namespace Constellation.Core.DomainEvents;

using System;

public sealed record StudentAddedToGroupTutorialDomainEvent(
    Guid Id,
    Guid TutorialId,
    Guid EnrolmentId)
    : DomainEvent(Id);
