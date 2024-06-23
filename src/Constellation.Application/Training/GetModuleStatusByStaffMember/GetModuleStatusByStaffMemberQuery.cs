namespace Constellation.Application.Training.GetModuleStatusByStaffMember;

using Abstractions.Messaging;
using System.Collections.Generic;

public sealed record GetModuleStatusByStaffMemberQuery(
        string StaffId)
    : IQuery<List<ModuleStatusResponse>>;