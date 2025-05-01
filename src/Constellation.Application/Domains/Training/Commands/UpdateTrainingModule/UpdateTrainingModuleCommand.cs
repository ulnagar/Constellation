namespace Constellation.Application.Domains.Training.Commands.UpdateTrainingModule;

using Abstractions.Messaging;
using Core.Enums;
using Core.Models.Training.Identifiers;

public sealed record UpdateTrainingModuleCommand(
    TrainingModuleId Id,
    string Name,
    TrainingModuleExpiryFrequency Expiry,
    string Url)
    : ICommand;
