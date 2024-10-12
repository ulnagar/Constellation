namespace Constellation.Core.Models.Enrolments;

using Constellation.Core.Models.Identifiers;
using Constellation.Core.Models.Offerings.Identifiers;
using Constellation.Core.Models.Students.Identifiers;
using Errors;
using Events;
using Identifiers;
using Primitives;
using Shared;
using System;

public class Enrolment : AggregateRoot, IAuditableEntity
{

    private Enrolment(
        StudentId studentId,
        OfferingId offeringId)
    {
        Id = new();
        StudentId = studentId;
        OfferingId = offeringId;
    }

    public EnrolmentId Id { get; private set; }
    public StudentId StudentId { get; private set; }
    public OfferingId OfferingId { get; private set; }
    public bool IsDeleted { get; private set; }
    public string CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public string ModifiedBy { get; set; }
    public DateTime ModifiedAt { get; set; }
    public string DeletedBy { get; set; }
    public DateTime DeletedAt { get; set; }

    public static Enrolment Create(
        StudentId studentId,
        OfferingId offeringId)
    {
        Enrolment entry = new(studentId, offeringId);

        entry.RaiseDomainEvent(new EnrolmentCreatedDomainEvent(new DomainEventId(), entry.Id, studentId, offeringId));

        return entry;
    }

    public Result Cancel()
    {
        if (IsDeleted)
            return Result.Failure(EnrolmentErrors.AlreadyDeleted);

        IsDeleted = true;

        RaiseDomainEvent(new EnrolmentDeletedDomainEvent(new DomainEventId(), Id, StudentId, OfferingId));

        return Result.Success();
    }
}