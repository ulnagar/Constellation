namespace Constellation.Core.DomainEvents;

using System;

public sealed record TutorialRollSubmittedDomainEvent(
    Guid Id,
    Guid TutorialId,
    Guid RollId) : DomainEvent(Id);
