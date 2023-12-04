﻿namespace Constellation.Application.MandatoryTraining.UpdateTrainingCompletion;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.DTOs;
using Core.Models.Training.Identifiers;
using System;

public sealed record UpdateTrainingCompletionCommand(
    TrainingCompletionId CompletionId,
    string StaffId,
    TrainingModuleId TrainingModuleId,
    DateOnly CompletedDate,
    bool NotRequired,
    FileDto File) 
    : ICommand;
