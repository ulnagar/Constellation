#nullable enable
namespace Constellation.Core.Models.Attendance;

using Enums;
using Identifiers;
using Offerings;
using Offerings.Identifiers;
using Primitives;
using Students;
using Students.Identifiers;
using Subjects;
using Subjects.Identifiers;
using System;
using System.Collections.Generic;
using System.Linq;
using Timetables;
using Timetables.Enums;
using Timetables.Identifiers;
using Timetables.ValueObjects;
using ValueObjects;

public sealed class AttendancePlan : AggregateRoot, IFullyAuditableEntity
{
    private readonly List<AttendancePlanPeriod> _periods = new();

    private AttendancePlan() { } // Required for EF Core

    private AttendancePlan(
        Student student)
    {
        Id = new();

        StudentId = student.Id;
        Student = student.Name;
        Grade = student.CurrentEnrolment!.Grade;
        SchoolCode = student.CurrentEnrolment!.SchoolCode;
        School = student.CurrentEnrolment!.SchoolName;
    }

    public AttendancePlanId Id { get; private set; }
    public StudentId StudentId { get; private set; }
    public Name Student { get; private set; }
    public Grade Grade { get; private set; }
    public string SchoolCode { get; private set; }
    public string School { get; private set; }
    public IReadOnlyList<AttendancePlanPeriod> Periods => _periods.AsReadOnly();
    public IDictionary<string, double> Percentages => CalculatePercentages();

    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string ModifiedBy { get; set; } = string.Empty;
    public DateTime ModifiedAt { get; set; }
    public bool IsDeleted { get; private set; }
    public string DeletedBy { get; set; } = string.Empty;
    public DateTime DeletedAt { get; set; }

    public static AttendancePlan Create(
        Student student)
    {
        AttendancePlan plan = new(
            student);

        return plan;
    }

    public void AddPeriods(
        List<Period> periods,
        Offering offering,
        Course course)
    {
        foreach (Period period in periods)
        {
            AttendancePlanPeriod planPeriod = new(
                period,
                offering,
                course);

            _periods.Add(planPeriod);
        }
    }

    private Dictionary<string, double> CalculatePercentages()
    {
        Dictionary<string, double> percentages = new();

        IEnumerable<IGrouping<CourseId, AttendancePlanPeriod>> periods = _periods.GroupBy(period => period.CourseId);

        foreach (IGrouping<CourseId, AttendancePlanPeriod> periodGroup in periods)
        {
            double target = periodGroup.First().TargetMinutesPerCycle;

            double total = periodGroup.Sum(period => period.MinutesPresent);

            double percentage = (target / total) * 100;

            percentages.Add(periodGroup.First().CourseName, percentage);
        }

        return percentages;
    }
}

public sealed class AttendancePlanPeriod
{
    private AttendancePlanPeriod() { } // Required for EF Core

    internal AttendancePlanPeriod(
        Period period,
        Offering offering,
        Course course)
    {
        PeriodId = period.Id;
        Timetable = period.Timetable;
        Week = period.Week;
        Day = period.Day;
        PeriodName = period.Name;
        PeriodType = period.Type;
        StartTime = period.StartTime;
        EndTime = period.EndTime;

        OfferingId = offering.Id;
        OfferingName = offering.Name;

        CourseId = course.Id;
        CourseName = course.Name;
        TargetMinutesPerCycle = course.TargetMinutesPerCycle;
    }

    public AttendancePlanId PlanId { get; private set; }

    // Period Details
    public PeriodId PeriodId { get; private set; }
    public Timetable Timetable { get; private set; }
    public PeriodWeek Week { get; private set; }
    public PeriodDay Day { get; private set; }
    public string PeriodName { get; private set; }
    public PeriodType PeriodType { get; private set; }
    public TimeSpan StartTime { get; private set; }
    public TimeSpan EndTime { get; private set; }

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
