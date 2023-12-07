namespace Constellation.Application.Training.Roles.UpdateTrainingRole;

using Abstractions.Messaging;
using Core.Models.Training.Identifiers;
using Models;

public sealed record UpdateTrainingRoleCommand(
        TrainingRoleId RoleId,
        string Name)
    : ICommand<TrainingRoleResponse>;