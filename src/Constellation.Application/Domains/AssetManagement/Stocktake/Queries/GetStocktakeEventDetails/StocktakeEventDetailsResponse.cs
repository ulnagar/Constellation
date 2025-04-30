namespace Constellation.Application.Domains.AssetManagement.Stocktake.Queries.GetStocktakeEventDetails;

using System;
using System.Collections.Generic;

public sealed record StocktakeEventDetailsResponse(
    Guid Id,
    string Name,
    DateTime StartDate,
    DateTime EndDate,
    List<StocktakeEventDetailsResponse.Sighting> Sightings)
{
    public sealed record Sighting(
        Guid Id,
        string AssetNumber,
        string SerialNumber,
        string Description,
        string Location,
        string User,
        bool IsCancelled,
        string SightedBy,
        DateTime SightedOn);
};
