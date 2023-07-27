namespace Constellation.Core.ValueObjects;

using Constellation.Core.Primitives;
using Constellation.Core.Shared;
using System.Collections.Generic;

public sealed class CoverTeacherType : ValueObject
{
    public static readonly CoverTeacherType Casual = new("Casual");
    public static readonly CoverTeacherType Staff = new("Staff");

    public static CoverTeacherType ByValue(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        if (value == Casual.Value) return Casual;
        if (value == Staff.Value) return Staff;

        return null;
    }

    private CoverTeacherType(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }
}
