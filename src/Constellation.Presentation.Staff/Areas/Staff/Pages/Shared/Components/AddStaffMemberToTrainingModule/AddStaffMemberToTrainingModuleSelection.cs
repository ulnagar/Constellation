namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Shared.Components.AddStaffMemberToTrainingModule;

using Constellation.Core.Models.Training.Identifiers;
using Core.Models.StaffMembers.Identifiers;

public class AddStaffMemberToTrainingModuleSelection
{
    public TrainingModuleId ModuleId { get; set; }
    public string ModuleName { get; set; }

    public StaffId StaffId { get; set; }
    public Dictionary<StaffId, string> StaffMembers { get; set; } = new();
}
