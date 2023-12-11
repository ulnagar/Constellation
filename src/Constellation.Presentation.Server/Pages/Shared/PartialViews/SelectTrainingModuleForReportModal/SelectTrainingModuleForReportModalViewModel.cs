namespace Constellation.Presentation.Server.Pages.Shared.PartialViews.SelectTrainingModuleForReportModal;

public class SelectTrainingModuleForReportModalViewModel
{
    public Guid ModuleId { get; set; }
    public Dictionary<Guid, string> Modules { get; set; }
    public bool DetailedReportRequested { get; set; }
}
