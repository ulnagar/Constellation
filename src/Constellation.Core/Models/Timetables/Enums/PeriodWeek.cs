namespace Constellation.Core.Models.Timetables.Enums;

using Common;
using System.Collections.Generic;
using System.Linq;

public class PeriodWeek : IntEnumeration<PeriodWeek>
{
    public static readonly PeriodWeek WeekA = new(1, "Week A");
    public static readonly PeriodWeek WeekB = new(2, "Week B");

    private PeriodWeek(int value, string name)
        : base(value, name) { }

    public static IEnumerable<PeriodWeek> GetOptions 
        => Enumerations.Select(entry => entry.Value).ToList();
}