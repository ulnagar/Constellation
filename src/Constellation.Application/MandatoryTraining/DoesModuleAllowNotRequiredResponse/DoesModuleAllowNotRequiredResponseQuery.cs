namespace Constellation.Application.MandatoryTraining.DoesModuleAllowNotRequiredResponse;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.Identifiers;

public sealed record DoesModuleAllowNotRequiredResponseQuery(
    TrainingModuleId ModuleId)
    : IQuery<bool>;
