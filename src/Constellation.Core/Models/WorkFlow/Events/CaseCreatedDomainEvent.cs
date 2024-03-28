namespace Constellation.Core.Models.WorkFlow.Events;

using DomainEvents;
using Identifiers;
using Models.Identifiers;

public sealed record CaseCreatedDomainEvent(
    DomainEventId Id,
    CaseId CaseId)
    : DomainEvent(Id);