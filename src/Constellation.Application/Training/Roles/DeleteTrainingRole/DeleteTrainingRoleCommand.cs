namespace Constellation.Application.Training.Roles.DeleteTrainingRole;

using Abstractions.Messaging;
using Core.Models.Training.Identifiers;

public sealed record DeleteTrainingRoleCommand(
        TrainingRoleId RoleId)
    : ICommand;
