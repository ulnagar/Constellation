namespace Constellation.Core.Models.Training.Contexts.Roles;
using Identifiers;

public sealed class TrainingRoleMember
{
    private TrainingRoleMember() { }

    private TrainingRoleMember(
        TrainingRoleId roleId,
        string staffId)
    {
        RoleId = roleId;
        StaffId = staffId;
    }

    public TrainingRoleId RoleId { get; private set; }
    public TrainingRole Role { get; private set; }
    public string StaffId { get; private set; }

    public static TrainingRoleMember Create(
        TrainingRoleId roleId,
        string staffId)
    {
        TrainingRoleMember member = new TrainingRoleMember(roleId, staffId);

        return member;
    }
}