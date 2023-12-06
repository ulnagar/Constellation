namespace Constellation.Application.Training.Modules.UpdateTrainingModule;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Enums;
using Core.Models.Training.Identifiers;

public sealed record UpdateTrainingModuleCommand(
    TrainingModuleId Id,
    string Name,
    TrainingModuleExpiryFrequency Expiry,
    string Url,
    bool CanMarkNotRequired)
    : ICommand;
