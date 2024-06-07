namespace Constellation.Application.Stocktake.GetStocktakeEventList;

using Abstractions.Messaging;
using Models;
using System.Collections.Generic;

public sealed record GetStocktakeEventListQuery()
    : IQuery<List<StocktakeEventResponse>>;