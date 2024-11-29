namespace Constellation.Core.Models.Attendance;

using Identifiers;
using Students.Identifiers;
using System;
using System.Collections.Generic;
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
    private AttendancePlanPeriod(
        int periodId,
        string timetable,
        int day,
        int period,
        TimeSpan startTime,
        TimeSpan endTime)
    {
        PeriodId = periodId;
        Timetable = timetable;
        Day = day;
        Period = period;
        StartTime = startTime;
        EndTime = endTime;
    }

    public int PeriodId { get; private set; }
    public string Timetable { get; private set; }
    public int Day { get; private set; }
    public int Period { get; private set; }
    public string Name { get; private set; }
    public string Type { get; private set; }
    public TimeSpan StartTime { get; private set; }
    public TimeSpan EndTime { get; private set; }
    public TimeSpan EntryTime { get; private set; }
    public TimeSpan ExitTime { get; private set; }


}
