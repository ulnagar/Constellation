namespace Constellation.Core.Models.Offerings.Enums;

using Common;

public sealed class AssignmentType : StringEnumeration<AssignmentType>
{
    public static readonly AssignmentType ClassroomTeacher = new("Classroom Teacher");
    public static readonly AssignmentType Supervisor = new("Supervisor");
    public static readonly AssignmentType SupportTeacher = new("Support Teacher");
    public static readonly AssignmentType PracTeacher = new("Prac Teacher");
    public static readonly AssignmentType TutorialTeacher = new("Tutorial Teacher");

    public AssignmentType(string value)
    : base(value, value)
    {
    }

    public static IEnumerable<AssignmentType> GetOptions => GetEnumerable;
}