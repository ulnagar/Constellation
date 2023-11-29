namespace Constellation.Application.StaffMembers.GetStaffList;

using Abstractions.Messaging;
using System.Collections.Generic;

public sealed record GetStaffListQuery()
    : IQuery<List<StaffResponse>>;