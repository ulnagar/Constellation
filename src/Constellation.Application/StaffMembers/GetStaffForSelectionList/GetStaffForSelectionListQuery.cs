namespace Constellation.Application.StaffMembers.GetStaffForSelectionList;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.StaffMembers.Models;
using System.Collections.Generic;

public sealed record GetStaffForSelectionListQuery
    : IQuery<List<StaffSelectionListResponse>>;
