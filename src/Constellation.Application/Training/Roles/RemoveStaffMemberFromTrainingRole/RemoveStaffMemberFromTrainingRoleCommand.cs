namespace Constellation.Application.Training.Roles.RemoveStaffMemberFromTrainingRole;

using Abstractions.Messaging;
using Core.Models.Training.Identifiers;

public sealed record RemoveStaffMemberFromTrainingRoleCommand(
    TrainingRoleId RoleId,
    string StaffId)
    : ICommand;
