namespace Constellation.Application.Assets.GetAllocationList;

using Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using System.Collections.Generic;

public sealed record GetCommunityMemberAllocationListQuery()
    : IQuery<List<AllocationListItem>>;