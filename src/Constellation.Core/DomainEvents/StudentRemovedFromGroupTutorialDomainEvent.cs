namespace Constellation.Core.DomainEvents;

using System;

public sealed record StudentRemovedFromGroupTutorialDomainEvent(
    Guid Id,
    Guid TutorialId,
    Guid EnrolmentId)
    : DomainEvent(Id);