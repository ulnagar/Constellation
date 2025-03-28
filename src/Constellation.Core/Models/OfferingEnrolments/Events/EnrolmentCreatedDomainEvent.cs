namespace Constellation.Core.Models.OfferingEnrolments.Events;

using Constellation.Core.DomainEvents;
using Constellation.Core.Models.Identifiers;
using Constellation.Core.Models.Offerings.Identifiers;
using Constellation.Core.Models.Students.Identifiers;
using Identifiers;

public sealed record EnrolmentCreatedDomainEvent(
    DomainEventId Id,
    OfferingEnrolmentId OfferingEnrolmentId,
    StudentId StudentId,
    OfferingId OfferingId)
    : DomainEvent(Id);