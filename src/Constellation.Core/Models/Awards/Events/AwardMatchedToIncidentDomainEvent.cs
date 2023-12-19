namespace Constellation.Core.Models.Awards.Events;

using DomainEvents;
using Identifiers;

public sealed record AwardMatchedToIncidentDomainEvent(
        DomainEventId Id,
        StudentAwardId AwardId)
    : DomainEvent(Id);