namespace Constellation.Core.Models.Enrolments;

using Errors;
using Events;
using Identifiers;
using Constellation.Core.Models.Identifiers;
using Constellation.Core.Models.Offerings.Identifiers;
using Constellation.Core.Models.Subjects.Identifiers;
using Primitives;
using Shared;
using System;

public class Enrolment : AggregateRoot, IAuditableEntity
{

    private Enrolment(
        string studentId,
        OfferingId offeringId)
    {
        Id = new();
        StudentId = studentId;
        OfferingId = offeringId;
    }

    public EnrolmentId Id { get; private set; }
    public string StudentId { get; private set; }
    public OfferingId OfferingId { get; private set; }
    public bool IsDeleted { get; private set; }
    public string CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public string ModifiedBy { get; set; }
    public DateTime ModifiedAt { get; set; }
    public string DeletedBy { get; set; }
    public DateTime DeletedAt { get; set; }

    public static Enrolment Create(
        string studentId,
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