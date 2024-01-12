namespace Constellation.Application.Stocktake.GetCurrentStocktakeEvents;

using System;

public sealed record StocktakeEventResponse(
    Guid Id,
    string Name,
    DateTime StartDate,
    DateTime EndDate);