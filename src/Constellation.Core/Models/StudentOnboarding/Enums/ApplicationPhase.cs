namespace Constellation.Core.Models.StudentOnboarding.Enums;

using System.ComponentModel.DataAnnotations;

public enum ApplicationPhase
{
    [Display(Name = "Data Entry")]
    DataEntry,
    Placement,
    Approval,
    Processing
}