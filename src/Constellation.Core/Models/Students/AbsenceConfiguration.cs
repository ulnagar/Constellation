namespace Constellation.Core.Models.Students;

using Constellation.Core.Errors;
using Constellation.Core.Models.Absences;
using Constellation.Core.Models.Identifiers;
using Constellation.Core.Primitives;
using Constellation.Core.Shared;
using Errors;
using System;

public class AbsenceConfiguration: IAuditableEntity
{
    private AbsenceConfiguration(
        string studentId,
        AbsenceType absenceType,
        DateOnly scanStartDate,
        DateOnly scanEndDate)
    {
        Id = new();
        StudentId = studentId;
        AbsenceType = absenceType;

        ScanStartDate = scanStartDate;
        ScanEndDate = scanEndDate;
        CalendarYear = scanStartDate.Year;
    }

    public StudentAbsenceConfigurationId Id { get; private set; }
    public string StudentId { get; private set; }
    public int CalendarYear { get; private set; }
    public AbsenceType AbsenceType { get; private set; }
    public DateOnly ScanStartDate { get; private set; }
    public DateOnly ScanEndDate { get; private set; }

    public string CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public string ModifiedBy { get; set; }
    public DateTime ModifiedAt { get; set; }

    public bool IsDeleted { get; private set; }

    public string DeletedBy { get; set; }
    public DateTime DeletedAt { get; set; }

    public static Result<AbsenceConfiguration> Create(
        string studentId,
        AbsenceType type,
        DateOnly startDate,
        DateOnly? endDate)
    {
        if (endDate.HasValue && endDate.Value < startDate)
            return Result.Failure<AbsenceConfiguration>(ValidationErrors.Date.RangeReversed(startDate, endDate.Value));

        if (string.IsNullOrWhiteSpace(studentId))
            return Result.Failure<AbsenceConfiguration>(ValidationErrors.String.RequiredIsNull(nameof(studentId)));

        return new AbsenceConfiguration(
            studentId,
            type,
            startDate,
            (endDate.HasValue) ? endDate.Value : new DateOnly(DateTime.Today.Year, 12, 31));
    }

    public Result Cancel(DateOnly cancelDate)
    {
        if (cancelDate < ScanStartDate || cancelDate > ScanEndDate)
            return Result.Failure(ValidationErrors.Date.OutOfRange(cancelDate, ScanStartDate, ScanEndDate));

        if (IsDeleted)
            return Result.Failure(AbsenceConfigurationErrors.AlreadyCancelled);

        ScanEndDate = cancelDate;

        return Result.Success();
    }

    public void Delete()
    {
        ScanEndDate = DateOnly.FromDateTime(DateTime.Today);

        IsDeleted = true;
    }
}
