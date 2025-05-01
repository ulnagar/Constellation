namespace Constellation.Application.Domains.Training.Commands.UpdateTrainingCompletion;

using Abstractions.Messaging;
using Core.Models.Training.Identifiers;
using DTOs;
using System;

public sealed record UpdateTrainingCompletionCommand(
    TrainingCompletionId CompletionId,
    string StaffId,
    TrainingModuleId TrainingModuleId,
    DateOnly CompletedDate,
    FileDto File)
    : ICommand;
