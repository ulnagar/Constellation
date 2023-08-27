namespace Constellation.Core.Models.Enrolments.Events;

using Constellation.Core.DomainEvents;
using Constellation.Core.Models.Enrolments.Identifiers;
using Constellation.Core.Models.Identifiers;

public sealed record EnrolmentCreatedDomainEvent(
    DomainEventId Id,
    EnrolmentId EnrolmentId)
    : DomainEvent(Id);