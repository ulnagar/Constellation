namespace Constellation.Presentation.Shared.Pages.Shared.Components.AddModuleToTrainingRole;

using Constellation.Core.Models.Training.Identifiers;

public class AddModuleToTrainingRoleSelection
{
    public TrainingRoleId RoleId { get; set; }
    public string RoleName { get; set; }

    public Guid ModuleId { get; set; }
    public Dictionary<Guid, string> Modules { get; set; }
}
