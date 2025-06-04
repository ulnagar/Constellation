namespace Constellation.Application.Domains.WorkFlows.Commands.CreateTrainingCase;

using Abstractions.Messaging;
using Core.Models.StaffMembers.Identifiers;
using Core.Models.Training.Identifiers;

public sealed record CreateTrainingCaseCommand(
    StaffId StaffId,
    TrainingModuleId ModuleId,
    TrainingCompletionId? CompletionId)
    : ICommand;