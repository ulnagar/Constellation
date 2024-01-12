namespace Constellation.Application.Stocktake.GetCurrentStocktakeEvents;

using Abstractions.Messaging;
using System.Collections.Generic;

public sealed record GetCurrentStocktakeEventsQuery 
    : IQuery<List<StocktakeEventResponse>>;