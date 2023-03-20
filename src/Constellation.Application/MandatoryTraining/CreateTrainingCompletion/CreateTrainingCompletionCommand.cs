namespace Constellation.Application.MandatoryTraining.CreateTrainingCompletion;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.DTOs;
using Constellation.Core.Models.Identifiers;
using System;

public sealed record CreateTrainingCompletionCommand(
    string StaffId,
    TrainingModuleId TrainingModuleId,
    DateTime CompletedDate,
    bool NotRequired,
    FileDto File)
    : ICommand;
