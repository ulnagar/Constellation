namespace Constellation.Core.Models.AppSettings.Enums;

using Core.Common;

public sealed class WorkflowArea : StringEnumeration<WorkflowArea>
{
    public static readonly WorkflowArea Attendance = new("Attendance", "Attendance");
    public static readonly WorkflowArea Compliance = new("Compliance", "Compliance");
    public static readonly WorkflowArea Training = new("Training", "Training");

    private WorkflowArea(string value, string name)
        : base(value, name) { }
}