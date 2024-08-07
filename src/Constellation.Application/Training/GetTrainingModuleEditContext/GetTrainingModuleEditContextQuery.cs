﻿namespace Constellation.Application.Training.GetTrainingModuleEditContext;

using Constellation.Application.Abstractions.Messaging;
using Core.Models.Training.Identifiers;

public sealed record GetTrainingModuleEditContextQuery(
    TrainingModuleId Id)
    : IQuery<ModuleEditContextDto>;
