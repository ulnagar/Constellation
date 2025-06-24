namespace Constellation.Application.Domains.AssetManagement.Stocktake.Queries.GetStocktakeEvent;

using Abstractions.Messaging;
using Core.Models.Stocktake.Identifiers;
using Models;

public sealed record GetStocktakeEventQuery(
    StocktakeEventId EventId)
    : IQuery<StocktakeEventResponse>;