namespace Constellation.Core.Models.Students;

using Abstractions.Clock;
using Constellation.Core.Enums;
using Constellation.Core.Errors;
using Errors;
using Identifiers;
using Primitives;
using Shared;
using System;

public sealed class SchoolEnrolment : IAuditableEntity
{
    private SchoolEnrolment(
        StudentId studentId,
        string schoolCode,
        string schoolName,
        Grade grade,
        int year,
        DateOnly startDate,
        DateOnly? endDate)
    {
        Id = new();
        StudentId = studentId;
        SchoolCode = schoolCode;
        SchoolName = schoolName;
        Grade = grade;
        Year = year;
        StartDate = startDate;
        EndDate = endDate;
    }

    public SchoolEnrolmentId Id { get; private set; }
    public StudentId StudentId { get; private set; }
    public string SchoolCode { get; private set; }
    public string SchoolName { get; private set; }
    public Grade Grade { get; private set; }
    public int Year { get; private set; }
    public DateOnly StartDate { get; private set; }
    public DateOnly? EndDate { get; private set; }

    public string CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public string ModifiedBy { get; set; }
    public DateTime ModifiedAt { get; set; }
    public bool IsDeleted { get; private set; }
    public string DeletedBy { get; set; }
    public DateTime DeletedAt { get; set; }

    internal static Result<SchoolEnrolment> Create(
        StudentId studentId,
        string schoolCode,
        string schoolName,
        Grade grade,
        int year,
        DateOnly? startDate,
        DateOnly? endDate,
        IDateTimeProvider dateTime)
    {
        if (studentId == StudentId.Empty)
            return Result.Failure<SchoolEnrolment>(StudentErrors.InvalidId);

        if (string.IsNullOrWhiteSpace(schoolCode) || string.IsNullOrWhiteSpace(schoolName))
            return Result.Failure<SchoolEnrolment>(DomainErrors.Partners.School.NotFound(schoolCode));

        startDate ??= dateTime.Today;
        
        return new SchoolEnrolment(
            studentId,
            schoolCode,
            schoolName,
            grade,
            year,
            startDate.Value,
            endDate);
    }

    internal void Delete(
        IDateTimeProvider dateTime)
    {
        EndDate = dateTime.Today;
        IsDeleted = true;
    }
}