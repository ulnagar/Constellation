namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Shared.Components.RejectAttendancePlanModal;

using System.ComponentModel.DataAnnotations;

public sealed class RejectAttendancePlanModalSelection
{
    [Required]
    [MinLength(5, ErrorMessage = "You must enter a longer comment")]
    public string Comment { get; set; } = string.Empty;

    public bool SendEmailUpdate { get; set; }
}
