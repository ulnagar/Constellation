namespace Constellation.Application.Training.GetModuleDetails;

using Constellation.Application.Abstractions.Messaging;
using Core.Models.Training.Identifiers;
using Models;

public sealed record GetModuleDetailsQuery(
    TrainingModuleId Id)
    : IQuery<ModuleDetailsDto>;
