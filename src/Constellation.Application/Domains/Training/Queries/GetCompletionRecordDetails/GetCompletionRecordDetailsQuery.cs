namespace Constellation.Application.Domains.Training.Queries.GetCompletionRecordDetails;

using Abstractions.Messaging;
using Core.Models.Training.Identifiers;
using Models;

public sealed record GetCompletionRecordDetailsQuery(
    TrainingModuleId ModuleId,
    TrainingCompletionId CompletionId)
    : IQuery<CompletionRecordDto>;
