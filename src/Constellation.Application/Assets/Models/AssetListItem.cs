namespace Constellation.Application.Assets.Models;

using Core.Models.Assets.Enums;
using Core.Models.Assets.Identifiers;
using Core.Models.Assets.ValueObjects;

public sealed record AssetListItem(
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