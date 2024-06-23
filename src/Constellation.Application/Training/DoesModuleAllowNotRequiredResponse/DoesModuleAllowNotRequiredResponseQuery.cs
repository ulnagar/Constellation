namespace Constellation.Application.Training.DoesModuleAllowNotRequiredResponse;

using Constellation.Application.Abstractions.Messaging;
using Core.Models.Training.Identifiers;

public sealed record DoesModuleAllowNotRequiredResponseQuery(
    string StaffId,
    TrainingModuleId ModuleId)
    : IQuery<bool>;
