namespace Constellation.Application.Training.Roles.GetTrainingRoleDetails;

using Abstractions.Messaging;
using Constellation.Core.Models.Training.Identifiers;

public sealed record GetTrainingRoleDetailsQuery(
    TrainingRoleId RoleId)
    : IQuery<TrainingRoleDetailResponse>;