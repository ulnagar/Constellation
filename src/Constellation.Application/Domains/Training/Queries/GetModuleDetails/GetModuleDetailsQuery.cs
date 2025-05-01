namespace Constellation.Application.Domains.Training.Queries.GetModuleDetails;

using Abstractions.Messaging;
using Core.Models.Training.Identifiers;
using Models;

public sealed record GetModuleDetailsQuery(
    TrainingModuleId Id)
    : IQuery<ModuleDetailsDto>;
