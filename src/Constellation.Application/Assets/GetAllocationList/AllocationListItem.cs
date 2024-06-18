namespace Constellation.Application.Assets.GetAllocationList;

using Core.Models.Assets.Enums;
using Core.Models.Assets.Identifiers;
using Core.Models.Assets.ValueObjects;

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
