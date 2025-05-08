namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Shared.Components.ShowDashboardWidgets;

public sealed class ShowDashboardWidgetsViewComponentModel
{
    public bool ShowTrainingWidgets { get; set; }
    public int WithoutRole { get; set; }

    public bool ShowAbsenceWidgets { get; set; }
    public int PartialScanDisabled { get; set; }
    public int WholeScanDisabled { get; set; }

    public bool ShowAwardsWidgets { get; set; }
    public int AwardOverages { get; set; }
    public int AwardAdditions { get; set; }

    public bool ShowSentralIdWidgets { get; set; }
    public int StudentsWithoutSentralId { get; set; }

    public int ActiveWorkFlowActions { get; set; }

    public int PendingAttendancePlans { get; set; }
    public int ProcessingAttendancePlans { get; set; }

    public int EdvalDifferences { get; set; }
}
