namespace Constellation.Application.Training.Modules.CreateTrainingCompletion;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.DTOs;
using Core.Models.Training.Identifiers;
using System;

public sealed record CreateTrainingCompletionCommand(
    string StaffId,
    TrainingModuleId TrainingModuleId,
    DateOnly CompletedDate,
    FileDto File)
    : ICommand;
