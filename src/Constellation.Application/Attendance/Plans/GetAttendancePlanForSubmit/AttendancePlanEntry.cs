namespace Constellation.Application.Attendance.Plans.GetAttendancePlanForSubmit;

using Core.Enums;
using Core.Models.Attendance.Enums;
using Core.Models.Attendance.Identifiers;
using Core.Models.Students.Identifiers;
using Core.Models.Timetables.Enums;
using Core.Models.Timetables.ValueObjects;
using Core.ValueObjects;
using System;
using System.Collections.Generic;

public sealed record AttendancePlanEntry(
    AttendancePlanId PlanId,
    AttendancePlanStatus Status,
    StudentId StudentId,
    Name Student,
    Grade Grade,
    string SchoolCode,
    string School,
    List<AttendancePlanEntry.PlanPeriod> Periods,
    List<AttendancePlanEntry.FreePeriod> FreePeriods,
    List<AttendancePlanEntry.MissedPeriod> MissedPeriods,
    AttendancePlanEntry.SciencePracLesson? SciencePrac)
{

    public sealed record PlanPeriod(
        AttendancePlanPeriodId PlanPeriodId,
        Timetable Timetable,
        PeriodWeek Week,
        PeriodDay Day,
        string PeriodName,
        PeriodType PeriodType,
        TimeOnly StartTime,
        TimeOnly EndTime,
        string OfferingName,
        string CourseName,
        TimeOnly EntryTime,
        TimeOnly ExitTime);

    public sealed record FreePeriod(
        PeriodWeek Week,
        PeriodDay Day,
        string Period,
        double Minutes,
        string Activity);

    public sealed record MissedPeriod(
        string Subject,
        double TotalMinutesPerCycle,
        double MinutesMissedPerCycle,
        double PercentMissed);

    public sealed record SciencePracLesson(
        PeriodWeek Week,
        PeriodDay Day,
        string Period);
}