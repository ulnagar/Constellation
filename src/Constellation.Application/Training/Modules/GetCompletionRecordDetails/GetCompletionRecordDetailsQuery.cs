namespace Constellation.Application.Training.Modules.GetCompletionRecordDetails;

using Constellation.Application.Abstractions.Messaging;
using Core.Models.Training.Identifiers;
using Models;

public sealed record GetCompletionRecordDetailsQuery(
    TrainingModuleId ModuleId,
    TrainingCompletionId CompletionId)
    : IQuery<CompletionRecordDto>;
