namespace Constellation.Core.Models.Training.Contexts.Roles;

using Identifiers;

public sealed class TrainingRoleModule
{
    private TrainingRoleModule() { }

    private TrainingRoleModule(
        TrainingRoleId roleId,
        TrainingModuleId moduleId)
    {
        RoleId = roleId;
        ModuleId = moduleId;
    }

    public TrainingRoleId RoleId { get; private set; }
    public TrainingRole Role { get; private set; }
    public TrainingModuleId ModuleId { get; private set; }

    public static TrainingRoleModule Create(
        TrainingRoleId roleId,
        TrainingModuleId moduleId)
    {
        TrainingRoleModule module = new(roleId, moduleId);

        return module;
    }
}