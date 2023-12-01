namespace Constellation.Application.MandatoryTraining.MarkTrainingCompletionRecordDeleted;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.MandatoryTraining.Identifiers;

public sealed record MarkTrainingCompletionRecordDeletedCommand(
    TrainingModuleId ModuleId,
    TrainingCompletionId CompletionId)
    : ICommand;
