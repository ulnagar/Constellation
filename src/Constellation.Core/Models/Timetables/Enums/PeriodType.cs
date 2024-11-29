namespace Constellation.Core.Models.Timetables.Enums;

using Constellation.Core.Common;

public class PeriodType : StringEnumeration<PeriodType>
{
    public static readonly PeriodType Teaching = new("T", "Teaching");
    public static readonly PeriodType Break = new("B", "Break");
    public static readonly PeriodType Offline = new("O", "Offline");

    private PeriodType(string value, string name)
        : base(value, name)
    { }
}