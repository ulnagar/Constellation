namespace Constellation.Core.Models.StudentOnboarding.Enums;

using Core.Common;

public sealed class ApplicationPhase : StringEnumeration<ApplicationPhase>
{
    public static readonly ApplicationPhase DataEntry = new("DataEntry", "Data Entry");
    public static readonly ApplicationPhase Placement = new("Placement", "Placement");
    public static readonly ApplicationPhase Approval = new("Approval", "Approval");
    public static readonly ApplicationPhase Processing = new("Processing", "Processing");

    public ApplicationPhase(string value, string name)
        : base(value, name) { }

    public static IEnumerable<ApplicationPhase> GetOptions => GetEnumerable;
}