namespace Constellation.Application.Domains.Training.Commands.CreateTrainingCompletion;

using Abstractions.Messaging;
using Core.Models.StaffMembers.Identifiers;
using Core.Models.Training.Identifiers;
using DTOs;
using System;

public sealed record CreateTrainingCompletionCommand(
    StaffId StaffId,
    TrainingModuleId TrainingModuleId,
    DateOnly CompletedDate,
    FileDto File)
    : ICommand;
