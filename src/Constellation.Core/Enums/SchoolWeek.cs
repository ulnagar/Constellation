namespace Constellation.Core.Enums;

using Constellation.Core.Common;
using System.Collections.Generic;

public sealed class SchoolWeek : StringEnumeration<SchoolWeek>
{
    public static readonly SchoolWeek Empty = new("", "", 0);

    public static readonly SchoolWeek Week1 = new("1", "Week 1", 1);
    public static readonly SchoolWeek Week2 = new("2", "Week 2", 2);
    public static readonly SchoolWeek Week3 = new("3", "Week 3", 3);
    public static readonly SchoolWeek Week4 = new("4", "Week 4", 4);
    public static readonly SchoolWeek Week5 = new("5", "Week 5", 5);
    public static readonly SchoolWeek Week6 = new("6", "Week 6", 6);
    public static readonly SchoolWeek Week7 = new("7", "Week 7", 7);
    public static readonly SchoolWeek Week8 = new("8", "Week 8", 8);
    public static readonly SchoolWeek Week9 = new("9", "Week 9", 9);
    public static readonly SchoolWeek Week10 = new("10", "Week 10", 10);
    public static readonly SchoolWeek Week11 = new("11", "Week 11", 11);

    public int SortOrder { get; init; }

    private SchoolWeek(string value, string name, int order)
        : base(value, name)
    {
        SortOrder = order;
    }

    public static IEnumerable<SchoolWeek> GetOptions => GetEnumerable;
}
