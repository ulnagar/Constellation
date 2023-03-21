namespace Constellation.Application.MandatoryTraining.GetTrainingModuleEditContext;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.Identifiers;

public sealed record GetTrainingModuleEditContextQuery(
    TrainingModuleId Id)
    : IQuery<ModuleEditContextDto>;
