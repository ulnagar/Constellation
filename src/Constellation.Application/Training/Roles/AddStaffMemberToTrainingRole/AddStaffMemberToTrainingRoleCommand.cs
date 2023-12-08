namespace Constellation.Application.Training.Roles.AddStaffMemberToTrainingRole;

using Abstractions.Messaging;
using Core.Models.Training.Identifiers;
using System.Collections.Generic;

public sealed record AddStaffMemberToTrainingRoleCommand(
        TrainingRoleId RoleId,
        List<string> StaffIds)
    : ICommand;