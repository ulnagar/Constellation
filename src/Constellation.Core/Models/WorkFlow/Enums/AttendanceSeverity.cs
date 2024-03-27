namespace Constellation.Core.Models.WorkFlow.Enums;

using Common;

public class AttendanceSeverity : StringEnumeration<AttendanceSeverity>
{
    public static readonly AttendanceSeverity BandOne = new("Band One", "Band One : Greater than 95%");
    public static readonly AttendanceSeverity BandNil = new("Band Nil", "Band Nil : Between 95% and 80%");
    public static readonly AttendanceSeverity BandTwo = new("Band Two", "Band Two : Between 80% and 75%");
    public static readonly AttendanceSeverity BandThree = new("Band Three", "Band Three : Between 75% and 50%");
    public static readonly AttendanceSeverity BandFour = new("Band Four", "Band Four : Below 50%");

    private AttendanceSeverity(string value, string name)
        : base(value, name)
    { }

    public static AttendanceSeverity FromAttendanceValue(decimal value) =>
        value switch
        {
            >= 95 => BandOne,
            >= 80 and < 95 => BandNil,
            >= 75 and < 80 => BandTwo,
            >= 50 and < 75 => BandThree,
            < 50 => BandFour
        };
}