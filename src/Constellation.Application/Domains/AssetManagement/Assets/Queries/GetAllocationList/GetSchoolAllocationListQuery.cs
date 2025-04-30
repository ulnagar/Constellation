namespace Constellation.Application.Domains.AssetManagement.Assets.Queries.GetAllocationList;

using Constellation.Application.Abstractions.Messaging;
using System.Collections.Generic;

public sealed record GetSchoolAllocationListQuery()
    : IQuery<List<AllocationListItem>>;