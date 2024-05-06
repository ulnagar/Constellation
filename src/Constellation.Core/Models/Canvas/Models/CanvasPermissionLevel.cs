namespace Constellation.Core.Models.Canvas.Models;

public readonly record struct CanvasPermissionLevel(string Value)
{
    public static readonly CanvasPermissionLevel Student = new CanvasPermissionLevel("StudentEnrolment");
    public static readonly CanvasPermissionLevel Teacher = new CanvasPermissionLevel("TeacherEnrolment");

    public override string ToString() => Value;
}
