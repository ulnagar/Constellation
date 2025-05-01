namespace Constellation.Application.Domains.Training.Models;

using Core.Models.Training.Identifiers;

public sealed record ModuleSummaryDto(
    TrainingModuleId Id,
    string Name,
    bool IsActive,
    string Expiry,
    string Url);