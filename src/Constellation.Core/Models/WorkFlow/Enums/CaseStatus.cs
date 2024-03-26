namespace Constellation.Core.Models.WorkFlow.Enums;

using Common;

public class CaseStatus : StringEnumeration<CaseStatus>
{
    public static readonly CaseStatus Open = new("Open", "Open");
    public static readonly CaseStatus PendingAction = new("Pending", "Pending Action");
    public static readonly CaseStatus Completed = new("Completed", "Completed");
    public static readonly CaseStatus Cancelled = new("Cancelled", "Cancelled");

    private CaseStatus(string value, string name)
        : base(value, name)
    { }
}