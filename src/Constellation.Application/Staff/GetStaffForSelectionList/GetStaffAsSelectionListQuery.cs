namespace Constellation.Application.Staff.GetForSelectionList;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Staff.GetStaffForSelectionList;
using System.Collections.Generic;

public sealed record GetStaffForSelectionListQuery
    : IQuery<List<StaffSelectionListResponse>>;
