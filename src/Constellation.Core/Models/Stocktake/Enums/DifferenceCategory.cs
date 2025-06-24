namespace Constellation.Core.Models.Stocktake.Enums;

using Common;
using System.Collections.Generic;

public sealed class DifferenceCategory : StringEnumeration<DifferenceCategory>
{
    public static readonly DifferenceCategory ManualEntry = new("Manual", "Manually Entered");
    public static readonly DifferenceCategory UpdatedEntry = new("Updated", "Updated Entry");
    public static readonly DifferenceCategory ConditionComment = new("Condition", "Condition Updated");

    private DifferenceCategory(string value, string name)
        : base(value, name) { }

    public static IEnumerable<DifferenceCategory> GetOptions => GetEnumerable;
}