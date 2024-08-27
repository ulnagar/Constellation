namespace Constellation.Core.Models.Grade;

using Abstractions.Clock;
using Constellation.Core.Models.Students.Identifiers;
using Identifiers;
using Primitives;
using Shared;
using System;
using System.Collections.Generic;
using System.Linq;

public sealed class Cohort : IAuditableEntity
{
    private readonly List<CohortEnrolment> _enrolments = new();

    private Cohort(
        Enums.Grade grade,
        DateOnly startDate,
        DateOnly endDate)
    {
        Id = new();
        Grade = grade;
        StartDate = startDate;
        EndDate = endDate;
    }

    public CohortId Id { get; private set; }
    public Enums.Grade Grade {get; private set; }
    public DateOnly StartDate { get; private set; }
    public DateOnly EndDate { get; private set; }
    public IReadOnlyCollection<CohortEnrolment> Enrolments => _enrolments.AsReadOnly();
    
    public string CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public string ModifiedBy { get; set; }
    public DateTime ModifiedAt { get; set; }

    /// <summary>
    /// Not used. Check EndDate instead.
    /// </summary>
    public bool IsDeleted { get; }
    public string DeletedBy { get; set; }
    public DateTime DeletedAt { get; set; }

    public static Result<Cohort> Create(
        Enums.Grade grade,
        DateOnly startDate,
        DateOnly endDate)
    {
        if (endDate < startDate)
            return Result.Failure<Cohort>();

        if (startDate > endDate)
            return Result.Failure<Cohort>();

        return new Cohort(
            grade,
            startDate,
            endDate);
    }

    public void Delete(
        IDateTimeProvider dateTime)
    {
        EndDate = dateTime.Today;
    }

    public Result AddEnrolment(
        StudentId studentId, 
        DateOnly? startDate,
        IDateTimeProvider dateTime)
    {
        if (_enrolments.Any(entry => entry.StudentId == studentId && !entry.IsDeleted))
            return Result.Failure();

        Result<CohortEnrolment> enrolment = CohortEnrolment.Create(
            studentId,
            Id,
            startDate ?? dateTime.Today);

        if (enrolment.IsFailure)
            return Result.Failure(enrolment.Error);

        _enrolments.Add(enrolment.Value);

        return Result.Success();
    }

    public Result RemoveEnrolment(
        StudentId studentId)
    {
        if (_enrolments.All(entry => entry.StudentId != studentId))
            return Result.Success();

        if (_enrolments.Where(entry => entry.StudentId == studentId).All(entry => entry.IsDeleted))
            return Result.Success();

        IEnumerable<CohortEnrolment> enrolments = _enrolments.Where(entry => entry.StudentId == studentId && !entry.IsDeleted);

        foreach (CohortEnrolment enrolment in enrolments)
            enrolment.Delete();
        
        return Result.Success();
    }
}