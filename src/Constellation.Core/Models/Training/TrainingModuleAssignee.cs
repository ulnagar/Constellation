namespace Constellation.Core.Models.Training;

using Identifiers;
using StaffMembers.Identifiers;

public sealed class TrainingModuleAssignee
{
    private TrainingModuleAssignee() { }

    private TrainingModuleAssignee(
        TrainingModuleId moduleId,
        StaffId staffId)
    {
        ModuleId = moduleId;
        StaffId = staffId;
    }

    public TrainingModuleId ModuleId { get; private set; }
    public StaffId StaffId { get; private set; }

    public static TrainingModuleAssignee Create(
        TrainingModuleId moduleId,
        StaffId staffId)
    {
        TrainingModuleAssignee member = new(moduleId, staffId);

        return member;
    }
}