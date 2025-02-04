namespace Constellation.Application.Training.Models;

using Constellation.Core.Models.Training.Identifiers;

public sealed record ModuleSummaryDto(
    TrainingModuleId Id,
    string Name,
    bool IsActive,
    string Expiry,
    string Url);