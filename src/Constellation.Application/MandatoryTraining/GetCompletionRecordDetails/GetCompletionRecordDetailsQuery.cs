namespace Constellation.Application.MandatoryTraining.GetCompletionRecordDetails;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.MandatoryTraining.Models;
using Core.Models.Training.Identifiers;

public sealed record GetCompletionRecordDetailsQuery(
    TrainingModuleId ModuleId,
    TrainingCompletionId CompletionId)
    : IQuery<CompletionRecordDto>;
