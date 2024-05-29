namespace Constellation.Presentation.Shared.Pages.Shared.PartialViews.SelectTrainingModuleForReportModal;

using System.ComponentModel.DataAnnotations;

public class SelectTrainingModuleForReportModalViewModel
{
    [Required]
    public Guid ModuleId { get; set; }
    public Dictionary<Guid, string> Modules { get; set; }
    public bool DetailedReportRequested { get; set; }
}
