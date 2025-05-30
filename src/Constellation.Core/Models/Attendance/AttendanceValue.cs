﻿namespace Constellation.Core.Models.Attendance;

using Constellation.Core.Models.Students.Identifiers;
using Core.Enums;
using Errors;
using Identifiers;
using Primitives;
using Shared;
using System;

public sealed class AttendanceValue : AggregateRoot
{
    private AttendanceValue() {}

    private AttendanceValue(
        StudentId studentId,
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

    public AttendanceValueId Id { get; private set; } = new();
    public StudentId StudentId { get; private set; } = StudentId.Empty;
    public Grade Grade { get; private set; }
    public DateOnly StartDate { get; private set; }
    public DateOnly EndDate { get; private set; }
    public string PeriodLabel { get; private set; } = string.Empty;

    public decimal PerMinuteYearToDatePercentage { get; private set; }
    public decimal PerMinuteWeekPercentage { get; private set; }
    public decimal PerDayYearToDatePercentage { get; private set; }
    public decimal PerDayWeekPercentage { get; private set; }

    public static Result<AttendanceValue> Create(
        StudentId studentId,
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
            return Result.Failure<AttendanceValue>(AttendanceValueErrors.CreateEmptyValues);

        if (startDate == DateOnly.MinValue || endDate == DateOnly.MaxValue)
            return Result.Failure<AttendanceValue>(AttendanceValueErrors.CreateMinimumDates);

        if (startDate > endDate)
            return Result.Failure<AttendanceValue>(AttendanceValueErrors.CreateDateRangeInvalid);

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
