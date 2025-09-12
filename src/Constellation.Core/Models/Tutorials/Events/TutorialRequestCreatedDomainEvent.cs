namespace Constellation.Core.Models.Tutorials.Events;

using Constellation.Core.DomainEvents;
using Constellation.Core.Models.Identifiers;
using Identifiers;

public sealed record TutorialRequestCreatedDomainEvent(
    DomainEventId Id,
    RequestId RequestId)
    : DomainEvent(Id);