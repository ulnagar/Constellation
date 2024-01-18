namespace Constellation.Core.Models.Operations.Enums;

using Common;

public sealed class CanvasUserType : StringEnumeration<CanvasUserType>
{
    public static readonly CanvasUserType Teacher = new("Teacher", "Teacher");
    public static readonly CanvasUserType Student = new("Student", "Student");

    private CanvasUserType(string value, string name) 
        : base(value, name)
    {
    }
}