namespace Constellation.Application.MandatoryTraining.GetModuleDetails;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Features.MandatoryTraining.Models;
using Constellation.Core.Models.Identifiers;

public sealed record GetModuleDetailsQuery(
    TrainingModuleId Id)
    : IQuery<ModuleDetailsDto>;
