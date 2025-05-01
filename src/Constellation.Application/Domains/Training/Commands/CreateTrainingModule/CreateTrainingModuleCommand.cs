namespace Constellation.Application.Domains.Training.Commands.CreateTrainingModule;

using Abstractions.Messaging;
using Core.Enums;

public sealed record CreateTrainingModuleCommand(
    string Name,
    TrainingModuleExpiryFrequency Expiry,
    string Url)
    : ICommand;
