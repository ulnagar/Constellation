namespace Constellation.Application.Domains.StaffMembers.Queries.GetStaffForSelectionList;

using Abstractions.Messaging;
using Models;
using System.Collections.Generic;

public sealed record GetStaffForSelectionListQuery
    : IQuery<List<StaffSelectionListResponse>>;
