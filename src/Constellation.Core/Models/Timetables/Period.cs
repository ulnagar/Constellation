#nullable enable
namespace Constellation.Core.Models.Timetables;

using Enums;
using Identifiers;
using Primitives;
using System;

public sealed class Period : AggregateRoot, IAuditableEntity
{
    private Period(
        Timetable timetable,
        int week,
        PeriodDay day,
        string name,
        PeriodType type,
        TimeSpan startTime,
        TimeSpan endTime)
    {
        Id = new();
        Timetable = timetable;
        Day = day;
        Name = name;
        Type = type;
        StartTime = startTime;
        EndTime = endTime;
    }

    public PeriodId Id { get; private set; }
    public Timetable Timetable { get; private set; }
    public int Week { get; private set; }
    public PeriodDay Day { get; private set; }
    public string Name { get; private set; }
    public PeriodType Type { get; private set; }
    public TimeSpan StartTime { get; private set; }
    public TimeSpan EndTime { get; private set; }
    public int Duration => (int)EndTime.Subtract(StartTime).TotalMinutes;
    public string SortOrder => $"{Timetable.Value}.{Day.Value.ToString().PadLeft(2, '0')}.{StartTime.Hours.ToString().PadLeft(2, '0')}";


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
        string name,
        PeriodType type,
        TimeSpan startTime,
        TimeSpan endTime)
    {
        return new(
            timetable,
            week,
            day,
            name,
            type,
            startTime,
            endTime);
    }

    public string GroupName()
    {
        string gridName = Timetable.Name.ToUpper();

        string weekName = $"Week {"ABCDE"[Week - 1]}";

        string dayName = Day.Name;

        return $"{gridName} {weekName} {dayName}";
    }

    public override string ToString() =>
        $"{GroupName()} - {Name}";
}
