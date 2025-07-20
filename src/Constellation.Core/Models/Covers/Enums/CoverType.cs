namespace Constellation.Core.Models.Covers.Enums;

using Constellation.Core.Common;
using System.Collections.Generic;

public sealed class CoverType : StringEnumeration<CoverType>
{
    public static readonly CoverType ClassCover = new("ClassCover", "Class Cover");
    public static readonly CoverType AccessCover = new("AccessCover", "Access Cover");

    private CoverType(string value, string name)
        : base(value, name) { }

    public static IEnumerable<CoverType> GetOptions => GetEnumerable;
}