namespace Constellation.Application.Domains.AssetManagement.Assets.Queries.GetAllocationList;

using Constellation.Core.Models.Assets.Enums;
using Constellation.Core.Models.Assets.Identifiers;
using Constellation.Core.Models.Assets.ValueObjects;

public sealed record AllocationListItem(
    string UserKey,
    string UserName,
    string UserGroup,
    AssetId AssetId,
    AssetNumber AssetNumber,
    string SerialNumber,
    string ModelDescription,
    AssetStatus Status,
    AllocationId? AllocationId,
    string AllocatedTo,
    LocationId? LocationId,
    string LocationType,
    string LocationName);
