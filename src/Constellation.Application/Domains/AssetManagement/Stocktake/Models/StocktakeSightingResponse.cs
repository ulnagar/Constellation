namespace Constellation.Application.Domains.AssetManagement.Stocktake.Models;

using Core.Models.Assets.ValueObjects;
using Core.Models.Stocktake.Identifiers;
using System;

public sealed record StocktakeSightingResponse(
    StocktakeSightingId Id,
    string SerialNumber,
    AssetNumber AssetNumber,
    string Description,
    string LocationName,
    string UserName,
    string SightedBy,
    DateTime SightedAt);