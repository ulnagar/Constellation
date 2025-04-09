namespace Constellation.Core.Enums;

using Constellation.Core.Common;
using System.Collections.Generic;

public sealed class SchoolTerm : StringEnumeration<SchoolTerm>
{
    public static readonly SchoolTerm Empty = new("", 99);

    public static readonly SchoolTerm Term1 = new("Term 1", 1);
    public static readonly SchoolTerm Term2 = new("Term 2", 2);
    public static readonly SchoolTerm Term3 = new("Term 3", 3);
    public static readonly SchoolTerm Term4 = new("Term 4", 4);

    public int SortOrder { get; init; }

    private SchoolTerm(string value, int order)
        : base(value, value)
    {
        SortOrder = order;
    }

    public static IEnumerable<SchoolTerm> GetOptions => GetEnumerable;
}
