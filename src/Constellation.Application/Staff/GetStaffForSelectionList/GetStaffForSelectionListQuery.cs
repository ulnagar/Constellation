namespace Constellation.Application.StaffMembers.GetStaffForSelectionList;

using Constellation.Application.Abstractions.Messaging;
using System.Collections.Generic;

public sealed record GetStaffForSelectionListQuery
    : IQuery<List<StaffSelectionListResponse>>;
