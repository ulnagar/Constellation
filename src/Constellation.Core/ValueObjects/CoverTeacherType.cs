using Constellation.Core.Primitives;
using System.Collections.Generic;

namespace Constellation.Core.ValueObjects;
public sealed class CoverTeacherType : ValueObject
{
    public static readonly CoverTeacherType Casual = new("Casual");
    public static readonly CoverTeacherType Staff = new("Staff");

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
