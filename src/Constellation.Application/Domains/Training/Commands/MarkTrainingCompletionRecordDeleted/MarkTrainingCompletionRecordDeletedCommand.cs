namespace Constellation.Application.Domains.Training.Commands.MarkTrainingCompletionRecordDeleted;

using Abstractions.Messaging;
using Core.Models.Training.Identifiers;

public sealed record MarkTrainingCompletionRecordDeletedCommand(
    TrainingModuleId ModuleId,
    TrainingCompletionId CompletionId)
    : ICommand;
