namespace Constellation.Application.Training.Modules.GenerateOverallReport;

using Core.Enums;
using Core.Models.Training.Identifiers;

public sealed record ModuleDetails(
    TrainingModuleId ModuleId,
    string Name,
    TrainingModuleExpiryFrequency Expiry);