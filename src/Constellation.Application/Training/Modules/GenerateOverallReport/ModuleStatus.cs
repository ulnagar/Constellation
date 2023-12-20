namespace Constellation.Application.Training.Modules.GenerateOverallReport;

using Core.Models.Training.Identifiers;
using System;

public sealed record ModuleStatus(
    TrainingModuleId ModuleId,
    bool Required,
    DateOnly? LastCompletionDate);