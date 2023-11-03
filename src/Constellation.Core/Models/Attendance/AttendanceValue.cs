namespace Constellation.Core.Models.Attendance;

using Enums;
using Errors;
using Identifiers;
using Primitives;
using Shared;
using System;

public sealed class AttendanceValue : AggregateRoot
{
    private AttendanceValue(
        string studentId,
        Grade grade,
        DateOnly startDate,
        DateOnly endDate,
        string periodLabel,
        decimal minYtd,
        decimal minWeek,
        decimal dayYtd,
        decimal dayWeek)
    {
        Id = new();

        StudentId = studentId;
        Grade = grade;
        StartDate = startDate;
        EndDate = endDate;
        PeriodLabel = periodLabel;
        PerMinuteYearToDatePercentage = minYtd;
        PerMinuteWeekPercentage = minWeek;
        PerDayYearToDatePercentage = dayYtd;
        PerDayWeekPercentage = dayWeek;
    }

    public AttendanceValueId Id { get; private set; }
    public string StudentId { get; private set; }
    public Grade Grade { get; private set; }
    public DateOnly StartDate { get; private set; }
    public DateOnly EndDate { get; private set; }
    public string PeriodLabel { get; private set; }

    public decimal PerMinuteYearToDatePercentage { get; private set; }
    public decimal PerMinuteWeekPercentage { get; private set; }
    public decimal PerDayYearToDatePercentage { get; private set; }
    public decimal PerDayWeekPercentage { get; private set; }

    public static Result<AttendanceValue> Create(
        string studentId,
        Grade grade,
        DateOnly startDate,
        DateOnly endDate,
        string periodLabel,
        decimal minYtd,
        decimal minWeek,
        decimal dayYtd,
        decimal dayWeek)
    {
        if (minYtd == 0 && minWeek == 0 && dayYtd == 0 && dayWeek == 0)
            return Result.Failure<AttendanceValue>(AttendanceValueErrors.Create.EmptyValues);

        if (startDate == DateOnly.MinValue || endDate == DateOnly.MaxValue)
            return Result.Failure<AttendanceValue>(AttendanceValueErrors.Create.MinimumDates);

        if (startDate > endDate)
            return Result.Failure<AttendanceValue>(AttendanceValueErrors.Create.DateRangeInvalid);

        AttendanceValue value = new(
            studentId,
            grade,
            startDate,
            endDate,
            periodLabel,
            minYtd,
            minWeek,
            dayYtd,
            dayWeek);
        
        return value;
    }
}
