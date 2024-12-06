#nullable enable
namespace Constellation.Core.Models.Timetables;

using Constellation.Core.Models.Timetables.ValueObjects;
using Enums;
using Identifiers;
using Primitives;
using Shared;
using System;
using System.Linq;

public sealed class Period : AggregateRoot, IAuditableEntity
{
    private Period(
        Timetable timetable,
        int week,
        PeriodDay day,
        int sequence,
        string name,
        PeriodType type,
        TimeSpan startTime,
        TimeSpan endTime)
    {
        Id = new();
        Timetable = timetable;
        Day = day;
        DaySequence = sequence;
        Name = name;
        Type = type;
        StartTime = startTime;
        EndTime = endTime;
    }

    public PeriodId Id { get; private set; }
    public Timetable Timetable { get; private set; }
    public int Week { get; private set; }
    public int DayNumber => ((Week - 1) * 5) + Day.Value;
    public PeriodDay Day { get; private set; }
    public int DaySequence { get; private set; }
    public string Name { get; private set; }
    public PeriodType Type { get; private set; }
    public TimeSpan StartTime { get; private set; }
    public TimeSpan EndTime { get; private set; }
    public int Duration => (int)EndTime.Subtract(StartTime).TotalMinutes;
    public string SortOrder => $"{Timetable.Code}.{Day.Value.ToString().PadLeft(2, '0')}.{StartTime.ToString("g")}";
    public string WeekName => $"Week {"AB"[Week - 1]}";


    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string ModifiedBy { get; set; } = string.Empty;
    public DateTime ModifiedAt { get; set; }
    public bool IsDeleted { get; private set; }
    public string DeletedBy { get; set; } = string.Empty;
    public DateTime DeletedAt { get; set; }

    public static Period Create(
        Timetable timetable,
        int week,
        PeriodDay day,
        int sequence,
        string name,
        PeriodType type,
        TimeSpan startTime,
        TimeSpan endTime)
    {
        return new(
            timetable,
            week,
            day,
            sequence,
            name,
            type,
            startTime,
            endTime);
    }

    public void Update(
        Timetable timetable,
        int week,
        PeriodDay day,
        int sequence,
        string name,
        PeriodType type,
        TimeSpan startTime,
        TimeSpan endTime)
    {
        Timetable = timetable;
        Week = week;
        Day = day;
        DaySequence = sequence;
        Name = name;
        Type = type;
        StartTime = startTime;
        EndTime = endTime;
    }
    
    public string GroupName() => 
        $"{Timetable.Name} {WeekName} {Day.Name}";

    public override string ToString() =>
        $"{GroupName()} - {Name}";

    public string SentralPeriodName()
    {
        char prefix = Timetable.Prefix;

        return $"{prefix}{DaySequence}";
    }
}
