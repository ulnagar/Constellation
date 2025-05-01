namespace Constellation.Application.Domains.Training.Queries.GetModuleStatusByStaffMember;

using Core.Enums;
using Core.Models.Training.Identifiers;
using System;

public sealed record ModuleStatusResponse(
    TrainingModuleId ModuleId,
    string ModuleName,
    TrainingModuleExpiryFrequency Expiry,
    bool IsRequired,
    bool IsCompleted,
    TrainingCompletionId? CompletionId,
    DateOnly? DateCompleted,
    DateOnly? DueDate);