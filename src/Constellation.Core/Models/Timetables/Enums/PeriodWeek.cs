namespace Constellation.Core.Models.Timetables.Enums;

using Common;
using System;
using System.Collections.Generic;
using System.Linq;

public class PeriodWeek : IntEnumeration<PeriodWeek>, IComparable<PeriodWeek>
{
    public static readonly PeriodWeek Unknown = new(0, "Unknown");
    public static readonly PeriodWeek WeekA = new(1, "Week A");
    public static readonly PeriodWeek WeekB = new(2, "Week B");

    /// <summary>
    /// Do not use. For serialization purposes only.
    /// </summary>
    private PeriodWeek() { }

    private PeriodWeek(int value, string name)
        : base(value, name) { }

    public static PeriodWeek FromDayNumber(int dayNumber) =>
        dayNumber switch
        {
            0 => Unknown,
            <= 5 => WeekA,
            >= 6 => WeekB
        };

    public static IEnumerable<PeriodWeek> GetOptions 
        => Enumerations
            .Select(entry => entry.Value)
            .Where(entry => entry.Value > 0);

    public int CompareTo(PeriodWeek other) => 
        Value.CompareTo(other.Value);
}