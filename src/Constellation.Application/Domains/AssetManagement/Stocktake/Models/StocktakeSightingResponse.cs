namespace Constellation.Application.Domains.AssetManagement.Stocktake.Models;

using System;

public sealed record StocktakeSightingResponse(
    Guid Id,
    string SerialNumber,
    string AssetNumber,
    string Description,
    string LocationName,
    string UserName,
    string SightedBy,
    DateTime SightedAt);