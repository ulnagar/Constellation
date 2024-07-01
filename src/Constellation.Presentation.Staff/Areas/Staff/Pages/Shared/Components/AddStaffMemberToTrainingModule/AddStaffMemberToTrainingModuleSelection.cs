namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Shared.Components.AddStaffMemberToTrainingModule;

using Constellation.Core.Models.Training.Identifiers;

public class AddStaffMemberToTrainingModuleSelection
{
    public TrainingModuleId ModuleId { get; set; }
    public string ModuleName { get; set; }

    public string StaffId { get; set; }
    public Dictionary<string, string> StaffMembers { get; set; } = new();
}
