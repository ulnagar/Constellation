namespace Constellation.Application.Training.Roles.RemoveModuleFromTrainingRole;

using Abstractions.Messaging;
using Core.Models.Training.Identifiers;

public sealed record RemoveModuleFromTrainingRoleCommand(
        TrainingRoleId RoleId,
        TrainingModuleId ModuleId)
    : ICommand;