namespace Constellation.Application.Domains.Training.Queries.DoesModuleAllowNotRequiredResponse;

using Abstractions.Messaging;
using Core.Models.StaffMembers.Identifiers;
using Core.Models.Training.Identifiers;

public sealed record DoesModuleAllowNotRequiredResponseQuery(
    StaffId StaffId,
    TrainingModuleId ModuleId)
    : IQuery<bool>;
