namespace Constellation.Application.Stocktake.GetCurrentStocktakeEvents;

using Abstractions.Messaging;
using Constellation.Application.Stocktake.Models;
using System.Collections.Generic;

public sealed record GetCurrentStocktakeEventsQuery 
    : IQuery<List<StocktakeEventResponse>>;