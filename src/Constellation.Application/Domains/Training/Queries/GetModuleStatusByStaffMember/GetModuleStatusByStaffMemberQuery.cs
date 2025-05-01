namespace Constellation.Application.Domains.Training.Queries.GetModuleStatusByStaffMember;

using Abstractions.Messaging;
using System.Collections.Generic;

public sealed record GetModuleStatusByStaffMemberQuery(
        string StaffId)
    : IQuery<List<ModuleStatusResponse>>;