namespace Constellation.Application.MandatoryTraining.GetModuleDetails;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.MandatoryTraining.Models;
using Core.Models.Training.Identifiers;

public sealed record GetModuleDetailsQuery(
    TrainingModuleId Id)
    : IQuery<ModuleDetailsDto>;
