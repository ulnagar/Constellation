namespace Constellation.Application.MandatoryTraining.GetCompletionRecordDetails;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.MandatoryTraining.Models;
using Constellation.Core.Models.MandatoryTraining.Identifiers;

public sealed record GetCompletionRecordDetailsQuery(
    TrainingModuleId ModuleId,
    TrainingCompletionId CompletionId)
    : IQuery<CompletionRecordDto>;
