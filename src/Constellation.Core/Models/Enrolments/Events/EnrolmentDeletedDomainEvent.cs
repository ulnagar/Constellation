namespace Constellation.Core.Models.Enrolments.Events;

using Constellation.Core.DomainEvents;
using Constellation.Core.Models.Enrolments.Identifiers;
using Constellation.Core.Models.Identifiers;
using Constellation.Core.Models.Subjects.Identifiers;

public sealed record EnrolmentDeletedDomainEvent(
    DomainEventId Id,
    EnrolmentId EnrolmentId,
    string StudentId,
    OfferingId OfferingId)
    : DomainEvent(Id);
