namespace Constellation.Core.Models.Canvas.Models;

public readonly record struct CanvasPermissionLevel(string Value)
{
    public static readonly CanvasPermissionLevel Student = new("StudentEnrollment");
    public static readonly CanvasPermissionLevel Teacher = new("TeacherEnrollment");

    public override string ToString() => Value;
}
