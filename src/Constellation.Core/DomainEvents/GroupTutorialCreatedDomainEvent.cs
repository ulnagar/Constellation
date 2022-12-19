namespace Constellation.Core.DomainEvents;

using System;

public sealed record GroupTutorialCreatedDomainEvent(
    Guid Id,
    Guid TutorialId)
    : DomainEvent(Id);
