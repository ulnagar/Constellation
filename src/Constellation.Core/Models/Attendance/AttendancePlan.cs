namespace Constellation.Core.Models.Attendance;

using Identifiers;
using Offerings.Identifiers;
using Students.Identifiers;
using Subjects.Identifiers;
using System;
using System.Collections.Generic;
using Timetables.Enums;
using Timetables.Identifiers;
using Timetables.ValueObjects;
using ValueObjects;

public sealed class AttendancePlan
{
    private readonly List<AttendancePlanPeriod> _periods = new();


    public AttendancePlanId Id { get; private set; }
    public StudentId StudentId { get; private set; }
    public Name Student { get; private set; }
    public string SchoolCode { get; private set; }
    public string School { get; private set; }
    public IReadOnlyList<AttendancePlanPeriod> Periods => _periods.AsReadOnly();

}

public sealed class AttendancePlanPeriod
{
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

    // Local Details
    public TimeOnly EntryTime { get; private set; }
    public TimeOnly ExitTime { get; private set; }
    public double MinutesPresent => (ExitTime - EntryTime).TotalMinutes;

}
