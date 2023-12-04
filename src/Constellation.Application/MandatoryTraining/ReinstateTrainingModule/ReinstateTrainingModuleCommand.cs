﻿namespace Constellation.Application.MandatoryTraining.ReinstateTrainingModule;

using Constellation.Application.Abstractions.Messaging;
using Core.Models.Training.Identifiers;

public sealed record ReinstateTrainingModuleCommand(
    TrainingModuleId Id)
    : ICommand;
