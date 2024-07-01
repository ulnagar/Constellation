namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Shared.Components.StaffTrainingReport;

public class StaffTrainingReportSelection
{
    public string StaffId { get; set; }
    public bool IncludeCertificates { get; set; }

    public Dictionary<string, string> StaffList { get; set; }
}
