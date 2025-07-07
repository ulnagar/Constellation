namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Shared.PartialViews.SelectStaffMemberForReportModal;

using Core.Models.StaffMembers.Identifiers;
using System.ComponentModel.DataAnnotations;

public class SelectStaffMemberForReportModalViewModel
{
    [Required]
    public StaffId StaffId { get; set; }
    public Dictionary<StaffId, string> StaffMembers { get; set; } = new();
    public ReportType Type { get; set; }
    
    public enum ReportType
    {
        Summary,
        Detail,
        Module
    }
}
