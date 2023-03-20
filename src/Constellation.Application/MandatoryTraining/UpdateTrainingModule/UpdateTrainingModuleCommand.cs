namespace Constellation.Application.MandatoryTraining.UpdateTrainingModule;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Enums;
using Constellation.Core.Models.Identifiers;

public sealed record UpdateTrainingModuleCommand(
    TrainingModuleId Id,
    string Name,
    TrainingModuleExpiryFrequency Expiry,
    string Url,
    bool CanMarkNotRequired)
    : ICommand;
