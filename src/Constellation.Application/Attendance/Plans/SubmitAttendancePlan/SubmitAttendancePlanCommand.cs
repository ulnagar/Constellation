namespace Constellation.Application.Attendance.Plans.SubmitAttendancePlan;

using Abstractions.Messaging;
using Core.Models.Attendance.Identifiers;
using Core.Models.Timetables.Enums;
using System;
using System.Collections.Generic;

public sealed record SubmitAttendancePlanCommand(
    AttendancePlanId PlanId,
    List<SubmitAttendancePlanCommand.PlanPeriod> Periods,
    SubmitAttendancePlanCommand.ScienceLesson? SciencePracLesson,
    List<SubmitAttendancePlanCommand.MissedLesson> MissedLessons,
    List<SubmitAttendancePlanCommand.FreePeriod> FreePeriods)
: ICommand
{
    public sealed record PlanPeriod(
        AttendancePlanPeriodId PlanPeriodId,
        TimeOnly EntryTime,
        TimeOnly ExitTime);

    public sealed record ScienceLesson(
        PeriodWeek Week,
        PeriodDay Day,
        string Period);

    public sealed record MissedLesson(
        string Subject,
        double TotalMinutesPerCycle,
        double MinutesMissedPerCycle);

    public sealed record FreePeriod(
        PeriodWeek Week,
        PeriodDay Day,
        string Period,
        double Minutes,
        string Activity);
}
