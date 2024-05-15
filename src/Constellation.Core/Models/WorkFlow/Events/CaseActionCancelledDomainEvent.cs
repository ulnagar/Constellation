namespace Constellation.Core.Models.WorkFlow.Events;

using Constellation.Core.Models.Identifiers;
using DomainEvents;
using Identifiers;

public sealed record CaseActionCancelledDomainEvent(
    DomainEventId Id,
    CaseId CaseId,
    ActionId ActionId)
    : DomainEvent(Id);