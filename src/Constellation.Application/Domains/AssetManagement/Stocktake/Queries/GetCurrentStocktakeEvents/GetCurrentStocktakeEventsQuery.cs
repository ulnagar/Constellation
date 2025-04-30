namespace Constellation.Application.Domains.AssetManagement.Stocktake.Queries.GetCurrentStocktakeEvents;

using Abstractions.Messaging;
using Constellation.Application.Domains.AssetManagement.Stocktake.Models;
using System.Collections.Generic;

public sealed record GetCurrentStocktakeEventsQuery 
    : IQuery<List<StocktakeEventResponse>>;