namespace Constellation.Application.Domains.Training.Queries.GenerateOverallReport;

using Core.Models.Training.Identifiers;
using System;

public sealed record ModuleStatus(
    TrainingModuleId ModuleId,
    bool Required,
    DateOnly? LastCompletionDate);