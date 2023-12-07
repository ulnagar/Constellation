namespace Constellation.Application.Training.Roles.GetTrainingRoleDetails;

using Constellation.Core.Enums;
using Core.Models.Training.Identifiers;
using Core.ValueObjects;
using System.Collections.Generic;

public sealed record TrainingRoleDetailResponse(
    TrainingRoleId RoleId,
    string Name,
    List<TrainingRoleDetailResponse.RoleMember> Members,
    List<TrainingRoleDetailResponse.RoleModule> Modules)
{
    public sealed record RoleMember(
        string StaffId,
        Name Name,
        string School);

    public sealed record RoleModule(
        TrainingModuleId ModuleId,
        string Name,
        TrainingModuleExpiryFrequency Expiry);
}
