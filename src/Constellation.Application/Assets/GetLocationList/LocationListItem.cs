namespace Constellation.Application.Assets.GetLocationList;

using Core.Models.Assets.Enums;
using Core.Models.Assets.Identifiers;
using Core.Models.Assets.ValueObjects;

public sealed record LocationListItem(
    LocationCategory LocationCategory,
    string LocationName,
    AssetId AssetId,
    AssetNumber AssetNumber,
    string SerialNumber,
    string ModelDescription,
    AssetStatus Status,
    AllocationId? AllocationId,
    string AllocatedTo);