namespace Constellation.Application.Domains.AssetManagement.Stocktake.Queries.ExportStocktakeSightingsAndDifferences;

using Core.Models.Assets.ValueObjects;
using Core.Models.Stocktake.Enums;
using System;

public sealed record StocktakeSightingWithDifferenceResponse(
    AssetNumber AssetNumber,
    string SerialNumber,
    string Description,
    LocationCategory LocationCategory,
    string LocationName,
    UserType UserType,
    string UserName,
    string Comment,
    string SightedBy,
    DateTime SightedAt,
    DifferenceCategory Difference);