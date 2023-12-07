namespace Constellation.Application.Training.Roles.GetTrainingRoleList;

using Abstractions.Messaging;
using Constellation.Application.Training.Models;
using System.Collections.Generic;

public sealed record GetTrainingRoleListQuery()
    : IQuery<List<TrainingRoleResponse>>;