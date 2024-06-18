namespace Constellation.Application.Assets.GetAllocationList;

using Abstractions.Messaging;
using System.Collections.Generic;

public sealed record GetSchoolAllocationListQuery()
    : IQuery<List<AllocationListItem>>;