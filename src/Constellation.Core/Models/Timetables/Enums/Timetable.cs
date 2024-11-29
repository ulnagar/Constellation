namespace Constellation.Core.Models.Timetables.Enums;

using Common;

public class Timetable : StringEnumeration<Timetable>
{
    public static readonly Timetable Primary = new("PRI", "Primary");
    public static readonly Timetable Junior6 = new("JU6", "Junior 6");
    public static readonly Timetable Junior8 = new("JU8", "Junior 8");
    public static readonly Timetable Senior = new("SEN", "Senior");
    
    private Timetable(string value, string name)
        : base(value, name) { }
}