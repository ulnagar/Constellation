namespace Constellation.Core.Models.Covers.Enums;

using Common;
using System.Collections.Generic;

public sealed class CoverTeacherType : StringEnumeration<CoverTeacherType>
{
    public static readonly CoverTeacherType Casual = new("Casual");
    public static readonly CoverTeacherType Staff = new("Staff");

    private CoverTeacherType(string value)
        : base(value, value) { }

    public static IEnumerable<CoverTeacherType> GetOptions => GetEnumerable;
}