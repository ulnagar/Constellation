namespace Constellation.Core.Models.Tutorials.Events;

using DomainEvents;
using Identifiers;
using Models.Identifiers;

public sealed record TutorialRequestRejectedDomainEvent(
    DomainEventId Id,
    RequestId RequestId)
    : DomainEvent(Id);