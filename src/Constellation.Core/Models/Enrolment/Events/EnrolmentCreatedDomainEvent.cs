namespace Constellation.Core.Models.Enrolment.Events;

using Constellation.Core.DomainEvents;
using Constellation.Core.Models.Enrolment.Identifiers;
using Constellation.Core.Models.Identifiers;

public sealed record EnrolmentCreatedDomainEvent(
    DomainEventId Id,
    EnrolmentId EnrolmentId)
    : DomainEvent(Id);