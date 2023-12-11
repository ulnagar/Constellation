﻿namespace Constellation.Application.Training.Modules.GetModuleStatusByStaffMember;

using Abstractions.Messaging;
using System.Collections.Generic;

public sealed record GetModuleStatusByStaffMemberQuery(
        string StaffId)
    : IQuery<List<ModuleStatusResponse>>;