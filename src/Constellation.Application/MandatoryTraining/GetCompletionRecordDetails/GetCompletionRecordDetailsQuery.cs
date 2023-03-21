namespace Constellation.Application.MandatoryTraining.GetCompletionRecordDetails;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.MandatoryTraining.Models;
using Constellation.Core.Models.Identifiers;

public sealed record GetCompletionRecordDetailsQuery(
    TrainingCompletionId Id)
    : IQuery<CompletionRecordDto>;
