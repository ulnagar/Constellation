﻿namespace Constellation.Presentation.Server.Pages.Shared.PartialViews.SelectStaffMemberForReportModal;

using System.ComponentModel.DataAnnotations;

public class SelectStaffMemberForReportModalViewModel
{
    [Required]
    public string StaffId { get; set; }
    public Dictionary<string, string> StaffMembers { get; set; } = new();
    public bool DetailedReportRequested { get; set; }

}
