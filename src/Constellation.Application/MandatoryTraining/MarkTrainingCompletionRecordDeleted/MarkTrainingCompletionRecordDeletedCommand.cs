namespace Constellation.Application.MandatoryTraining.MarkTrainingCompletionRecordDeleted;

using Constellation.Application.Abstractions.Messaging;
using Core.Models.Training.Identifiers;

public sealed record MarkTrainingCompletionRecordDeletedCommand(
    TrainingModuleId ModuleId,
    TrainingCompletionId CompletionId)
    : ICommand;
