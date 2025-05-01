namespace Constellation.Application.Domains.Training.Queries.GetCompletionRecordEditContext;

using Abstractions.Messaging;
using Core.Models.Training.Identifiers;

public sealed record GetCompletionRecordEditContextQuery(
    TrainingModuleId ModuleId,
    TrainingCompletionId CompletionId)
    : IQuery<CompletionRecordEditContextDto>;
