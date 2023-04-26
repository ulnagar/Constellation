namespace Constellation.Core.DomainEvents;

using Constellation.Core.Models.Identifiers;

public sealed record AwardCreatedDomainEvent(
    DomainEventId Id,
    StudentAwardId AwardId)
    : DomainEvent(Id);