namespace Constellation.Core.Models.WorkFlow.Events;

using DomainEvents;
using Identifiers;
using Models.Identifiers;

public sealed record CaseActionAssignedDomainEvent(
    DomainEventId Id,
    CaseId CaseId,
    ActionId ActionId,
    string StaffId)
    : DomainEvent(Id);