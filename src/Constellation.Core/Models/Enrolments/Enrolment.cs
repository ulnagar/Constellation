namespace Constellation.Core.Models.Enrolments;

using Constellation.Core.Models.Offerings.Identifiers;
using Constellation.Core.Models.Students.Identifiers;
using Errors;
using Events;
using Identifiers;
using Primitives;
using Shared;
using System;
using Tutorials.Identifiers;

public abstract class Enrolment : AggregateRoot, IAuditableEntity
{
    public EnrolmentId Id { get; protected set; }
    public StudentId StudentId { get; protected set; }
    public bool IsDeleted { get; private set; }
    public string CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public string ModifiedBy { get; set; }
    public DateTime ModifiedAt { get; set; }
    public string DeletedBy { get; set; }
    public DateTime DeletedAt { get; set; }

    public Result Cancel()
    {
        if (IsDeleted)
            return Result.Failure(EnrolmentErrors.AlreadyDeleted);

        IsDeleted = true;

        RaiseDomainEvent(new EnrolmentDeletedDomainEvent(new (), Id));

        return Result.Success();
    }
}

public sealed class OfferingEnrolment : Enrolment
{
    private OfferingEnrolment(
        StudentId studentId,
        OfferingId offeringId)
    {
        Id = new();
        StudentId = studentId;
        OfferingId = offeringId;
    }

    public OfferingId OfferingId { get; private set; }

    public static OfferingEnrolment Create(
        StudentId studentId,
        OfferingId offeringId)
    {
        OfferingEnrolment entry = new(studentId, offeringId);

        entry.RaiseDomainEvent(new EnrolmentCreatedDomainEvent(new (), entry.Id));

        return entry;
    }
}

public sealed class TutorialEnrolment : Enrolment
{
    private TutorialEnrolment(
        StudentId studentId,
        TutorialId tutorialId)
    {
        Id = new();
        StudentId = studentId;
        TutorialId = tutorialId;
    }

    public TutorialId TutorialId { get; private set; }

    public static TutorialEnrolment Create(
        StudentId studentId,
        TutorialId tutorialId)
    {
        TutorialEnrolment entry = new(studentId, tutorialId);

        entry.RaiseDomainEvent(new EnrolmentCreatedDomainEvent(new(), entry.Id));

        return entry;
    }
}
