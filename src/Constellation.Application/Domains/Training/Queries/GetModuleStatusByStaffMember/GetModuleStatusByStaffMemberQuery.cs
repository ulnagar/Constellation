namespace Constellation.Application.Domains.Training.Queries.GetModuleStatusByStaffMember;

using Abstractions.Messaging;
using Core.Models.StaffMembers.Identifiers;
using System.Collections.Generic;

public sealed record GetModuleStatusByStaffMemberQuery(
        StaffId StaffId)
    : IQuery<List<ModuleStatusResponse>>;