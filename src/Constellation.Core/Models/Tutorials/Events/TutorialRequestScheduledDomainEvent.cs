namespace Constellation.Core.Models.Tutorials.Events;

using DomainEvents;
using Identifiers;
using Models.Identifiers;

public sealed record TutorialRequestScheduledDomainEvent(
    DomainEventId Id,
    RequestId RequestId)
    : DomainEvent(Id);