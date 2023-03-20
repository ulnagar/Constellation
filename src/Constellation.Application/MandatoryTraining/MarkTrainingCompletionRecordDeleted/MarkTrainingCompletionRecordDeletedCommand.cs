namespace Constellation.Application.MandatoryTraining.MarkTrainingCompletionRecordDeleted;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.Identifiers;

public sealed record MarkTrainingCompletionRecordDeletedCommand(
    TrainingCompletionId RecordId)
    : ICommand;
