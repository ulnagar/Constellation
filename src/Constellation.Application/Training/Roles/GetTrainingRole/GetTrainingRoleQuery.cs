namespace Constellation.Application.Training.Roles.GetTrainingRole;

using Abstractions.Messaging;
using Core.Models.Training.Identifiers;
using Models;

public sealed record GetTrainingRoleQuery(
        TrainingRoleId RoleId)
    : IQuery<TrainingRoleResponse>;