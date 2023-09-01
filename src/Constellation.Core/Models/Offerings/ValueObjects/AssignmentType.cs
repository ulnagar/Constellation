namespace Constellation.Core.Models.Offerings.ValueObjects;

using Constellation.Core.Primitives;
using System.Collections.Generic;

public sealed class AssignmentType : ValueObject
{
    public static readonly AssignmentType ClassroomTeacher = new("Classroom Teacher");
    public static readonly AssignmentType Supervisor = new("Supervisor");
    public static readonly AssignmentType SupportTeacher = new("Support Teacher");
    public static readonly AssignmentType PracTeacher = new("Prac Teacher");
    public static readonly AssignmentType TutorialTeacher = new("Tutorial Teacher");

    public static AssignmentType FromValue(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        return new(value);
    }

    private AssignmentType(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }
}