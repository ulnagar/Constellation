namespace Constellation.Application.Stocktake.GetStocktakeEventDetails;

using Abstractions.Messaging;
using System;

public sealed record GetStocktakeEventDetailsQuery(
    Guid Id)
    : IQuery<StocktakeEventDetailsResponse>;