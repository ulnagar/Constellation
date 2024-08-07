﻿namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Shared.PartialViews.SelectTrainingModuleForReportModal;

using System.ComponentModel.DataAnnotations;

public class SelectTrainingModuleForReportModalViewModel
{
    [Required]
    public Guid ModuleId { get; set; }

    public Dictionary<Guid, string> Modules { get; set; } = new();
    public bool DetailedReportRequested { get; set; }
}
