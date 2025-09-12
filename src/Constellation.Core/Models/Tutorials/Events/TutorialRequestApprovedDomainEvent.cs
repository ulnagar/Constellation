namespace Constellation.Core.Models.Tutorials.Events;

using DomainEvents;
using Identifiers;
using Models.Identifiers;

public sealed record TutorialRequestApprovedDomainEvent(
    DomainEventId Id,
    RequestId RequestId)
    : DomainEvent(Id);