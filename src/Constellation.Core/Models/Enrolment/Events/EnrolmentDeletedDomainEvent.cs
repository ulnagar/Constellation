namespace Constellation.Core.Models.Enrolment.Events;

using Constellation.Core.DomainEvents;
using Constellation.Core.Models.Enrolment.Identifiers;
using Constellation.Core.Models.Identifiers;
using Constellation.Core.Models.Subjects.Identifiers;

public sealed record EnrolmentDeletedDomainEvent(
    DomainEventId Id,
    EnrolmentId EnrolmentId,
    string StudentId,
    OfferingId OfferingId)
    : DomainEvent(Id);
