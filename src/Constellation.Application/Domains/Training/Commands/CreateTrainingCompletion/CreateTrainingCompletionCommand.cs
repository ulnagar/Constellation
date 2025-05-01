namespace Constellation.Application.Domains.Training.Commands.CreateTrainingCompletion;

using Abstractions.Messaging;
using Core.Models.Training.Identifiers;
using DTOs;
using System;

public sealed record CreateTrainingCompletionCommand(
    string StaffId,
    TrainingModuleId TrainingModuleId,
    DateOnly CompletedDate,
    FileDto File)
    : ICommand;
