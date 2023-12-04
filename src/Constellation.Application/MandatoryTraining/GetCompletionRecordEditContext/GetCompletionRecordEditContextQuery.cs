namespace Constellation.Application.MandatoryTraining.GetCompletionRecordEditContext;

using Constellation.Application.Abstractions.Messaging;
using Core.Models.Training.Identifiers;

public sealed record GetCompletionRecordEditContextQuery(
    TrainingModuleId ModuleId,
    TrainingCompletionId CompletionId)
    : IQuery<CompletionRecordEditContextDto>;
