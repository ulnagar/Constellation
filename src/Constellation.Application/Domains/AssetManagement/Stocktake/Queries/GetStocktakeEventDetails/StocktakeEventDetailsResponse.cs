namespace Constellation.Application.Domains.AssetManagement.Stocktake.Queries.GetStocktakeEventDetails;

using Core.Models.Assets.ValueObjects;
using Core.Models.Stocktake.Identifiers;
using System;
using System.Collections.Generic;

public sealed record StocktakeEventDetailsResponse(
    StocktakeEventId Id,
    string Name,
    DateTime StartDate,
    DateTime EndDate,
    List<StocktakeEventDetailsResponse.Sighting> Sightings)
{
    public sealed record Sighting(
        StocktakeSightingId Id,
        AssetNumber AssetNumber,
        string SerialNumber,
        string Description,
        string Location,
        string User,
        bool IsCancelled,
        string SightedBy,
        DateTime SightedOn);
};
