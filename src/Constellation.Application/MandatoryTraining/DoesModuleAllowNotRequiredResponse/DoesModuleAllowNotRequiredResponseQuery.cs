namespace Constellation.Application.MandatoryTraining.DoesModuleAllowNotRequiredResponse;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.MandatoryTraining.Identifiers;

public sealed record DoesModuleAllowNotRequiredResponseQuery(
    string StaffId,
    TrainingModuleId ModuleId)
    : IQuery<bool>;
