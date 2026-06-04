namespace Constellation.Core.Models.StudentOnboarding.Enums;

using System.ComponentModel;

public enum ApplicationPhase
{
    [Description("Data Entry")]
    DataEntry,
    Placement,
    Approval,
    Processing
}