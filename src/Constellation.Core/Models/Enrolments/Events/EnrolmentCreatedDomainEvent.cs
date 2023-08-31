namespace Constellation.Core.Models.Enrolments.Events;

using Constellation.Core.DomainEvents;
using Constellation.Core.Models.Enrolments.Identifiers;
using Constellation.Core.Models.Identifiers;
using Constellation.Core.Models.Offerings.Identifiers;

public sealed record EnrolmentCreatedDomainEvent(
    DomainEventId Id,
    EnrolmentId EnrolmentId,
    string StudentId,
    OfferingId OfferingId)
    : DomainEvent(Id);