namespace Constellation.Presentation.Shared.Pages.Shared.PartialViews.SelectStaffMemberForReportModal;

using System.ComponentModel.DataAnnotations;

public class SelectStaffMemberForReportModalViewModel
{
    [Required]
    public string StaffId { get; set; }
    public Dictionary<string, string> StaffMembers { get; set; } = new();
    public ReportType Type { get; set; }
    
    public enum ReportType
    {
        Summary,
        Detail,
        Module
    }
}
