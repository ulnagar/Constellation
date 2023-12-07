namespace Constellation.Application.Training.Roles.AddStaffMemberToTrainingRole;

using Abstractions.Messaging;
using Core.Models.Training.Identifiers;

public sealed record AddStaffMemberToTrainingRoleCommand(
        TrainingRoleId RoleId,
        string StaffId)
    : ICommand;