﻿namespace Constellation.Application.Training.CreateTrainingModule;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Enums;

public sealed record CreateTrainingModuleCommand(
    string Name,
    TrainingModuleExpiryFrequency Expiry,
    string Url)
    : ICommand;
