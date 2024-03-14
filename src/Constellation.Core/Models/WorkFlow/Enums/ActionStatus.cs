namespace Constellation.Core.Models.WorkFlow.Enums;

using Constellation.Core.Common;

public class ActionStatus : StringEnumeration<ActionStatus>
{
    public static readonly ActionStatus Open = new("Open", "Open");
    public static readonly ActionStatus Completed = new("Completed", "Completed");
    public static readonly ActionStatus Cancelled = new("Cancelled", "Cancelled");

    private ActionStatus(string value, string name)
        : base(value, name)
    { }
}