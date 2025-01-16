namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Shared.Components.ApproveAttendancePlanModal;

using System.ComponentModel.DataAnnotations;

public sealed class ApproveAttendancePlanModalSelection
{
    [Required]
    [MinLength(5, ErrorMessage = "You must enter a longer comment")]
    public string Comment { get; set; } = string.Empty;
}
