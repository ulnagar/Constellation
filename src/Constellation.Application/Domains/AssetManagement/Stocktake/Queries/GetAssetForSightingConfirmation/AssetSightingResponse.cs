namespace Constellation.Application.Domains.AssetManagement.Stocktake.Queries.GetAssetForSightingConfirmation;

using Constellation.Core.Models.Assets.Identifiers;
using Constellation.Core.Models.Assets.ValueObjects;
using Constellation.Core.Models.Stocktake.Enums;

public sealed record AssetSightingResponse(
    AssetId AssetId,
    string SerialNumber,
    AssetNumber AssetNumber,
    string Description,
    LocationCategory LocationCategory,
    string LocationName,
    string LocationCode,
    UserType UserType,
    string UserName,
    string UserCode);
