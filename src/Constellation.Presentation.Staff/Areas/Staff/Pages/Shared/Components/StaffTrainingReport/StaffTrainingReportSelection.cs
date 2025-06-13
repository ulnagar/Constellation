namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Shared.Components.StaffTrainingReport;

using Core.Models.StaffMembers.Identifiers;

public class StaffTrainingReportSelection
{
    public StaffId StaffId { get; set; }
    public bool IncludeCertificates { get; set; }

    public Dictionary<StaffId, string> StaffList { get; set; }
}
