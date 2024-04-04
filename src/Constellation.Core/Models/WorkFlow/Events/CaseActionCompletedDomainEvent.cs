namespace Constellation.Core.Models.WorkFlow.Events;

using DomainEvents;
using Identifiers;
using Models.Identifiers;

public sealed record CaseActionCompletedDomainEvent(
    DomainEventId Id,
    CaseId CaseId,
    ActionId ActionId)
    : DomainEvent(Id);