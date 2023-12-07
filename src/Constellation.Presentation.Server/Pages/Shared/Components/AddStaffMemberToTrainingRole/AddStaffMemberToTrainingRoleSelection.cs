namespace Constellation.Presentation.Server.Pages.Shared.Components.AddStaffMemberToTrainingRole;

using Core.Models.Training.Identifiers;

public class AddStaffMemberToTrainingRoleSelection
{
    public TrainingRoleId RoleId { get; set; }
    public string RoleName { get; set; }

    public string StaffId { get; set; }
    public Dictionary<string, string> StaffMembers { get; set; } = new();
}
