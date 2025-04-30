namespace Constellation.Application.Domains.AssetManagement.Assets.Queries.GetLocationList;

using Constellation.Core.Models.Assets.Enums;
using Constellation.Core.Models.Assets.Identifiers;
using Constellation.Core.Models.Assets.ValueObjects;

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