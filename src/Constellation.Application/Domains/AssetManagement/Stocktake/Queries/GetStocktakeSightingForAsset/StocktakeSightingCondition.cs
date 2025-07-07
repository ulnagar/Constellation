namespace Constellation.Application.Domains.AssetManagement.Stocktake.Queries.GetStocktakeSightingForAsset;
public sealed record StocktakeSightingForAssetResponse(
    bool HasSighting,
    string AssetSchoolCode,
    string SightingSchoolCode);