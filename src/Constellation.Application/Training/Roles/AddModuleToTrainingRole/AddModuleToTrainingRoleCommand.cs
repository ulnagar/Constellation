namespace Constellation.Application.Training.Roles.AddModuleToTrainingRole;

using Abstractions.Messaging;
using Core.Models.Training.Identifiers;
using System.Collections.Generic;

public sealed record AddModuleToTrainingRoleCommand(
        TrainingRoleId RoleId,
        List<TrainingModuleId> ModuleIds)
    : ICommand;