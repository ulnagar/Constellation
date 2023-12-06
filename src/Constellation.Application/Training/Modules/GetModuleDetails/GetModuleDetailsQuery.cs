namespace Constellation.Application.Training.Modules.GetModuleDetails;

using Constellation.Application.Abstractions.Messaging;
using Core.Models.Training.Identifiers;
using Models;

public sealed record GetModuleDetailsQuery(
    TrainingModuleId Id)
    : IQuery<ModuleDetailsDto>;
