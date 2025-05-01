namespace Constellation.Application.Domains.Training.Queries.GetTrainingModuleEditContext;

using Abstractions.Messaging;
using Core.Models.Training.Identifiers;

public sealed record GetTrainingModuleEditContextQuery(
    TrainingModuleId Id)
    : IQuery<ModuleEditContextDto>;
