namespace Constellation.Core.Models.Enrolments.Events;

using Constellation.Core.Models.Identifiers;
using DomainEvents;
using Identifiers;

public sealed record EnrolmentCreatedDomainEvent(
    DomainEventId Id,
    EnrolmentId EnrolmentId)
    : DomainEvent(Id);