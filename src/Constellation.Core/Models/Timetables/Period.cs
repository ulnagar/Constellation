#nullable enable
namespace Constellation.Core.Models.Timetables;

using Constellation.Core.Models.Timetables.ValueObjects;
using Enums;
using Identifiers;
using Primitives;
using System;
using System.Linq;

public sealed class Period : AggregateRoot, IAuditableEntity
{
    private Period() { } // Required by EF Core

    private Period(
        Timetable timetable,
        PeriodWeek week,
        PeriodDay day,
        char periodCode,
        string name,
        PeriodType type,
        TimeSpan startTime,
        TimeSpan endTime)
    {
        Id = new();
        Timetable = timetable;
        Week = week;
        Day = day;
        PeriodCode = periodCode;
        Name = name;
        Type = type;
        StartTime = startTime;
        EndTime = endTime;
    }

    public PeriodId Id { get; private set; }
    public Timetable Timetable { get; private set; }
    public PeriodWeek Week { get; private set; }
    
    /// <summary>
    /// Day of the cycle
    /// </summary>
    public int DayNumber => ((Week.Value - 1) * 5) + Day.Value;
    public PeriodDay Day { get; private set; }
    
    /// <summary>
    ///  Period Code. e.g. 1 for Period 1, 0 for Period 0, R for Recess, etc
    /// </summary>
    public char PeriodCode { get; private set; }
    public string Name { get; private set; }
    public PeriodType Type { get; private set; }
    public TimeSpan StartTime { get; private set; }
    public TimeSpan EndTime { get; private set; }
    public int Duration => (int)EndTime.Subtract(StartTime).TotalMinutes;
    public string SortOrder => $"{Timetable.Code}.{Week.Value.ToString()}.{Day.Value.ToString().PadLeft(2, '0')}.{StartTime.ToString("g")}";

    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string ModifiedBy { get; set; } = string.Empty;
    public DateTime ModifiedAt { get; set; }
    public bool IsDeleted { get; private set; }
    public string DeletedBy { get; set; } = string.Empty;
    public DateTime DeletedAt { get; set; }

    public static Period Create(
        Timetable timetable,
        PeriodWeek week,
        PeriodDay day,
        char periodCode,
        string name,
        PeriodType type,
        TimeSpan startTime,
        TimeSpan endTime)
    {
        return new(
            timetable,
            week,
            day,
            periodCode,
            name,
            type,
            startTime,
            endTime);
    }

    public void Update(
        Timetable timetable,
        PeriodWeek week,
        PeriodDay day,
        char periodCode,
        string name,
        PeriodType type,
        TimeSpan startTime,
        TimeSpan endTime)
    {
        Timetable = timetable;
        Week = week;
        Day = day;
        PeriodCode = periodCode;
        Name = name;
        Type = type;
        StartTime = startTime;
        EndTime = endTime;
    }
    
    public string GroupName() => 
        $"{Timetable.Name} {Week.Name} {Day.Name}";

    public override string ToString() =>
        $"{GroupName()} - {Name}";

    public string SentralPeriodName()
    {
        char prefix = Timetable.Prefix;

        if (prefix == default)
            return PeriodCode.ToString();
        
        return $"{prefix}{PeriodCode}";
    }
}
