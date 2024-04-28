namespace Constellation.Core.Models.WorkFlow.Enums;

using Common;

public class AttendanceSeverity : StringEnumeration<AttendanceSeverity>
{
    public static readonly AttendanceSeverity BandZero = new("Band Zero", "Band Zero : Greater than 95%", 1);
    public static readonly AttendanceSeverity BandOne = new("Band One", "Band One : Between 95% and 80%", 2);
    public static readonly AttendanceSeverity BandTwo = new("Band Two", "Band Two : Between 80% and 75%", 3);
    public static readonly AttendanceSeverity BandThree = new("Band Three", "Band Three : Between 75% and 50%", 4);
    public static readonly AttendanceSeverity BandFour = new("Band Four", "Band Four : Below 50%", 5);

    private AttendanceSeverity(string value, string name, int order)
        : base(value, name, order)
    { }

    public static AttendanceSeverity FromAttendanceValue(decimal value) =>
        value switch
        {
            >= 95 => BandZero,
            >= 80 and < 95 => BandOne,
            >= 75 and < 80 => BandTwo,
            >= 50 and < 75 => BandThree,
            < 50 => BandFour
        };
}