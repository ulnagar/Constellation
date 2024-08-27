namespace Constellation.Core.Models.Grade;

using Constellation.Core.Models.Students.Identifiers;
using Identifiers;
using Primitives;
using Shared;
using System;

public sealed class CohortEnrolment : IAuditableEntity
{
    private CohortEnrolment(
        StudentId studentId,
        CohortId cohortId,
        DateOnly startDate)
    {
        Id = new();
        StudentId = studentId;
        CohortId = cohortId;
        StartDate = startDate;
    }

    public CohortEnrolmentId Id { get; private set; }
    public StudentId StudentId { get; private set; }
    public CohortId CohortId { get; private set; }
    public DateOnly StartDate { get; private set; }
    public DateOnly? EndDate => DeletedAt == DateTime.MinValue ? null : DateOnly.FromDateTime(DeletedAt);

    public string CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public string ModifiedBy { get; set; }
    public DateTime ModifiedAt { get; set; }
    public bool IsDeleted { get; private set; }
    public string DeletedBy { get; set; }
    public DateTime DeletedAt { get; set; }

    public static Result<CohortEnrolment> Create(
        StudentId studentId,
        CohortId cohortId,
        DateOnly startDate)
    {
        if (studentId == StudentId.Empty)
            return Result.Failure<CohortEnrolment>();

        if (cohortId == CohortId.Empty)
            return Result.Failure<CohortEnrolment>();
        
        return new CohortEnrolment(
            studentId,
            cohortId,
            startDate);
    }

    public void Delete()
    {
        IsDeleted = true;
    }
}