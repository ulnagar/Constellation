namespace Constellation.Application.Domains.AssetManagement.Stocktake.Queries.GetStocktakeSightingForAsset;

using Core.Models.Identifiers;

public sealed record StocktakeSightingForAssetResponse(
    bool HasSighting,
    SchoolCode AssetSchoolCode,
    string SightingSchoolCode);