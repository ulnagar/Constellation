namespace Constellation.Core.Models.Tutorials;

using Identifiers;
using Primitives;
using StaffMembers.Identifiers;
using System;
using Timetables.Enums;

public sealed class TutorialSession : IAuditableEntity
{
    internal TutorialSession(
        PeriodWeek week,
        PeriodDay day,
        TimeSpan startTime,
        TimeSpan endTime,
        StaffId staffId)
    {
        Id = new();

        Week = week;
        Day = day;
        StartTime = startTime;
        EndTime = endTime;
        StaffId = staffId;
    }

    public TutorialSessionId Id { get; private set; }
    public TutorialId TutorialId { get; private set; }
    public PeriodWeek Week { get; private set; }
    public int DayNumber => ((Week.Value - 1) * 5) + Day.Value;
    public PeriodDay Day { get; private set; }
    public TimeSpan StartTime { get; private set; }
    public TimeSpan EndTime { get; private set; }
    public int Duration => (int)EndTime.Subtract(StartTime).TotalMinutes;
    public string SortOrder => $"TUT.{Week.Value.ToString()}.{Day.Value.ToString().PadLeft(2, '0')}.{StartTime:g}";
    public StaffId StaffId { get; private set; }
    public bool IsDeleted { get; private set; }
    public string CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public string ModifiedBy { get; set; }
    public DateTime ModifiedAt { get; set; }
    public string DeletedBy { get; set; }
    public DateTime DeletedAt { get; set; }

    public override string ToString()
    {
        DateTime startTime = DateTime.Today.Add(StartTime);
        DateTime endTime = DateTime.Today.Add(EndTime);
        
        return $"Tutorial {Week.Name} {Day.Name} - {startTime:h:mm tt}-{endTime:h:mm tt}";
    }

    internal void Delete() => IsDeleted = true;
}