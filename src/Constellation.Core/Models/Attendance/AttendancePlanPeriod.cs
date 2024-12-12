#nullable enable
namespace Constellation.Core.Models.Attendance;

using Identifiers;
using Offerings;
using Offerings.Identifiers;
using Subjects;
using Subjects.Identifiers;
using System;
using System.Runtime.InteropServices.Marshalling;
using Timetables;
using Timetables.Enums;
using Timetables.Identifiers;
using Timetables.ValueObjects;

public sealed class AttendancePlanPeriod
{
    private AttendancePlanPeriod() { } // Required for EF Core

    internal AttendancePlanPeriod(
        AttendancePlanId planId,
        Period period,
        Offering offering,
        Course course)
    {
        Id = new();
        PlanId = planId;

        PeriodId = period.Id;
        Timetable = period.Timetable;
        Week = period.Week;
        Day = period.Day;
        PeriodName = period.Name;
        PeriodType = period.Type;
        StartTime = TimeOnly.FromTimeSpan(period.StartTime);
        EndTime = TimeOnly.FromTimeSpan(period.EndTime);

        OfferingId = offering.Id;
        OfferingName = offering.Name;

        CourseId = course.Id;
        CourseName = course.Name;
        TargetMinutesPerCycle = course.TargetMinutesPerCycle;
    }

    public AttendancePlanPeriodId Id { get; private set; }
    public AttendancePlanId PlanId { get; private set; }

    // Period Details
    public PeriodId PeriodId { get; private set; }
    public Timetable Timetable { get; private set; }
    public PeriodWeek Week { get; private set; }
    public PeriodDay Day { get; private set; }
    public string PeriodName { get; private set; }
    public PeriodType PeriodType { get; private set; }
    public TimeOnly StartTime { get; private set; }
    public TimeOnly EndTime { get; private set; }

    // Offering Details
    public OfferingId OfferingId { get; private set; }
    public string OfferingName { get; private set; }
    public CourseId CourseId { get; private set; }
    public string CourseName { get; private set; }
    public double TargetMinutesPerCycle { get; private set; }

    // Local Details
    public TimeOnly EntryTime { get; private set; } = TimeOnly.MinValue;
    public TimeOnly ExitTime { get; private set; } = TimeOnly.MinValue;
    public double MinutesPresent => (ExitTime - EntryTime).TotalMinutes;
    
    public void UpdateDetails(
        TimeOnly entryTime,
        TimeOnly exitTime)
    {
        EntryTime = entryTime;
        ExitTime = exitTime;
    }
}