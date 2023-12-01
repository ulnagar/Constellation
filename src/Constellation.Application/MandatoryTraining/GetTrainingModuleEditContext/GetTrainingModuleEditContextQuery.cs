namespace Constellation.Application.MandatoryTraining.GetTrainingModuleEditContext;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.MandatoryTraining.Identifiers;

public sealed record GetTrainingModuleEditContextQuery(
    TrainingModuleId Id)
    : IQuery<ModuleEditContextDto>;
