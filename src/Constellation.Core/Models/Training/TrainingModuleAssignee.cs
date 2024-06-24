namespace Constellation.Core.Models.Training;

using Identifiers;

public sealed class TrainingModuleAssignee
{
    private TrainingModuleAssignee() { }

    private TrainingModuleAssignee(
        TrainingModuleId moduleId,
        string staffId)
    {
        ModuleId = moduleId;
        StaffId = staffId;
    }

    public TrainingModuleId ModuleId { get; private set; }
    public string StaffId { get; private set; }

    public static TrainingModuleAssignee Create(
        TrainingModuleId moduleId,
        string staffId)
    {
        TrainingModuleAssignee member = new(moduleId, staffId);

        return member;
    }
}