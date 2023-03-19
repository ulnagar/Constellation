namespace Constellation.Core.DomainEvents;

using Constellation.Core.Models.Identifiers;
using System;

public sealed record TutorialRollSubmittedDomainEvent(
    DomainEventId Id,
    GroupTutorialId TutorialId,
    TutorialRollId RollId)
    : DomainEvent(Id);
