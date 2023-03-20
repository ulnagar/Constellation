namespace Constellation.Application.MandatoryTraining.UpdateTrainingCompletion;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.DTOs;
using Constellation.Core.Models.Identifiers;
using System;

public sealed record UpdateTrainingCompletionCommand(
    TrainingCompletionId Id,
    string StaffId,
    TrainingModuleId TrainingModuleId,
    DateTime CompletedDate,
    bool NotRequired,
    FileDto File) 
    : ICommand;
