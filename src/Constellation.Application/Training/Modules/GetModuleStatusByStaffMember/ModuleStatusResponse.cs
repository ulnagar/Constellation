﻿namespace Constellation.Application.Training.Modules.GetModuleStatusByStaffMember;

using Core.Enums;
using Core.Models.Training.Identifiers;
using System;
using System.Collections.Generic;

public sealed record ModuleStatusResponse(
    TrainingModuleId ModuleId,
    string ModuleName,
    TrainingModuleExpiryFrequency Expiry,
    bool IsRequired,
    Dictionary<TrainingRoleId, string> Roles,
    bool IsCompleted,
    TrainingCompletionId CompletionId,
    DateOnly? DateCompleted,
    DateOnly? DueDate);