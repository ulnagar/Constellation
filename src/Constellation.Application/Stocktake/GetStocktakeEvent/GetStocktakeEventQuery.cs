namespace Constellation.Application.Stocktake.GetStocktakeEvent;

using Abstractions.Messaging;
using Models;
using System;

public sealed record GetStocktakeEventQuery(
    Guid EventId)
    : IQuery<StocktakeEventResponse>;