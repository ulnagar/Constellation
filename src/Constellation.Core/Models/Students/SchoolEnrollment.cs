namespace Constellation.Core.Models.Students;

using Abstractions.Clock;
using Identifiers;
using Primitives;
using Shared;
using System;

public sealed class SchoolEnrollment : IAuditableEntity
{
    private SchoolEnrollment(
        StudentId studentId,
        string schoolCode,
        DateOnly startDate,
        DateOnly? endDate)
    {
        Id = new();
        StudentId = studentId;
        SchoolCode = schoolCode;
        StartDate = startDate;
        EndDate = endDate;
    }

    public SchoolEnrollmentId Id { get; private set; }
    public StudentId StudentId { get; private set; }
    public string SchoolCode { get; private set; }
    public DateOnly StartDate { get; private set; }
    public DateOnly? EndDate { get; private set; }

    public string CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public string ModifiedBy { get; set; }
    public DateTime ModifiedAt { get; set; }
    public bool IsDeleted { get; private set; }
    public string DeletedBy { get; set; }
    public DateTime DeletedAt { get; set; }

    public static Result<SchoolEnrollment> Create(
        StudentId studentId,
        string schoolCode,
        DateOnly? startDate,
        DateOnly? endDate,
        IDateTimeProvider dateTime)
    {
        if (studentId == StudentId.Empty)
            return Result.Failure<SchoolEnrollment>();

        if (string.IsNullOrWhiteSpace(schoolCode))
            return Result.Failure<SchoolEnrollment>();

        startDate ??= dateTime.Today;

        return new SchoolEnrollment(
            studentId,
            schoolCode,
            startDate.Value,
            endDate);
    }

    public void Delete(
        IDateTimeProvider dateTime)
    {
        EndDate = dateTime.Today;
        IsDeleted = true;
    }
}