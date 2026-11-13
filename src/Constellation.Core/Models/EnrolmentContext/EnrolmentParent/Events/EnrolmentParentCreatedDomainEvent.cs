namespace Constellation.Core.Models.EnrolmentContext.EnrolmentParent.Events;

using DomainEvents;
using Identifiers;
using Models.Identifiers;

public sealed record EnrolmentParentCreatedDomainEvent(
    DomainEventId Id,
    EnrolmentParentId ParentId)
    : DomainEvent(Id);