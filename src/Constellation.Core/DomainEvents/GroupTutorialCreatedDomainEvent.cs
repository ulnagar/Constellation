namespace Constellation.Core.DomainEvents;

using Constellation.Core.Models.Identifiers;
using System;

public sealed record GroupTutorialCreatedDomainEvent(
    DomainEventId Id,
    GroupTutorialId TutorialId)
    : DomainEvent(Id);
