namespace Constellation.Core.Models.AppSettings.Enums;

using Core.Common;

public sealed class TutorialPosition : StringEnumeration<TutorialPosition>
{
    public static readonly TutorialPosition Approver = new("Approver", "Tutorial Approver");
    public static readonly TutorialPosition Scheduler = new("Scheduler", "Tutorial Scheduler");
    private TutorialPosition(string value, string name)
        : base(value, name) { }
}