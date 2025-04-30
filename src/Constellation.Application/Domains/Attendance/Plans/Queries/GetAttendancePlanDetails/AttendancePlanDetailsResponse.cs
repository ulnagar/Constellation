namespace Constellation.Application.Domains.Attendance.Plans.Queries.GetAttendancePlanDetails;
using Core.Enums;
using Core.Models.Attendance.Enums;
using Core.Models.Attendance.Identifiers;
using Core.Models.Students.Identifiers;
using Core.Models.Timetables.Enums;
using Core.Models.Timetables.ValueObjects;
using Core.ValueObjects;
using System;
using System.Collections.Generic;

public sealed record AttendancePlanDetailsResponse(
    AttendancePlanId PlanId,
    AttendancePlanStatus Status,
    StudentId StudentId,
    Name Student,
    Grade Grade,
    string SchoolCode,
    string School,
    List<AttendancePlanDetailsResponse.NoteDetails> Notes,
    List<AttendancePlanDetailsResponse.PlanPeriod> Periods,
    List<AttendancePlanDetailsResponse.FreePeriod> FreePeriods,
    List<AttendancePlanDetailsResponse.MissedPeriod> MissedPeriods,
    AttendancePlanDetailsResponse.SciencePracLesson SciencePrac,
    List<AttendancePlanDetailsResponse.AlternatePercentage> AlternatePercentages)
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

    public sealed record NoteDetails(
        AttendancePlanNoteId NoteId,
        string Message,
        string CreatedBy,
        DateTime CreatedAt);

    public sealed record AlternatePercentage(
        string Course,
        string Class,
        double MinutesPresent,
        double Percentage);
}
