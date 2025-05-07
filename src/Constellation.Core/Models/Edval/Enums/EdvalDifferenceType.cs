namespace Constellation.Core.Models.Edval.Enums;

using Common;

public sealed class EdvalDifferenceType : StringEnumeration<EdvalDifferenceType>
{
    public static readonly EdvalDifferenceType None = new(string.Empty, string.Empty);

    public static readonly EdvalDifferenceType EdvalClass = new(nameof(EdvalClass), "Class");
    public static readonly EdvalDifferenceType EdvalClassMembership = new(nameof(EdvalClassMembership), "Class Membership");
    public static readonly EdvalDifferenceType EdvalStudent = new(nameof(EdvalStudent), "Student");
    public static readonly EdvalDifferenceType EdvalTeacher = new(nameof(EdvalTeacher), "Teacher");
    public static readonly EdvalDifferenceType EdvalTimetable = new(nameof(EdvalTimetable), "Timetable");
    
    public EdvalDifferenceType(string value, string name) 
        : base(value, name)
    { }
}