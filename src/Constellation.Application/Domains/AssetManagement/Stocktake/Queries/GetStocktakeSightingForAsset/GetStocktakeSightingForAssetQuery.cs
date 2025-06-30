namespace Constellation.Application.Domains.AssetManagement.Stocktake.Queries.GetStocktakeSightingForAsset;

using Abstractions.Messaging;
using Core.Models.Assets.ValueObjects;
using Core.Models.Stocktake.Identifiers;

public sealed record GetStocktakeSightingForAssetQuery(
    StocktakeEventId EventId,
    AssetNumber AssetNumber)
    : IQuery<StocktakeSightingForAssetResponse>;