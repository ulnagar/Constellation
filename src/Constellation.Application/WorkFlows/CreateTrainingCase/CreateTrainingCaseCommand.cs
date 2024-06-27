namespace Constellation.Application.WorkFlows.CreateTrainingCase;

using Abstractions.Messaging;
using Core.Models.Training.Identifiers;

public sealed record CreateTrainingCaseCommand(
    string StaffId,
    TrainingModuleId ModuleId,
    TrainingCompletionId? CompletionId)
    : ICommand;