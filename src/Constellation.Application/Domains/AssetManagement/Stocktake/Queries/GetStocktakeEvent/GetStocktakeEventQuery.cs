namespace Constellation.Application.Domains.AssetManagement.Stocktake.Queries.GetStocktakeEvent;

using Abstractions.Messaging;
using Models;
using System;

public sealed record GetStocktakeEventQuery(
    Guid EventId)
    : IQuery<StocktakeEventResponse>;