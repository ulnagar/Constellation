namespace Constellation.Application.Training.Roles.CreateTrainingRole;

using Abstractions.Messaging;
using Models;

public sealed record CreateTrainingRoleCommand(
    string Name)
    : ICommand<TrainingRoleResponse>;