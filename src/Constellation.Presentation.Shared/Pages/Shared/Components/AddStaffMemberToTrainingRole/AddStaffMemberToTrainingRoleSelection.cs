namespace Constellation.Presentation.Shared.Pages.Shared.Components.AddStaffMemberToTrainingRole;

using Constellation.Core.Models.Training.Identifiers;

public class AddStaffMemberToTrainingRoleSelection
{
    public TrainingRoleId RoleId { get; set; }
    public string RoleName { get; set; }

    public string StaffId { get; set; }
    public Dictionary<string, string> StaffMembers { get; set; } = new();
}
