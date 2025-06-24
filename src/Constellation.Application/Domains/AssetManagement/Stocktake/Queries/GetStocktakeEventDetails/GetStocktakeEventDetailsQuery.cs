namespace Constellation.Application.Domains.AssetManagement.Stocktake.Queries.GetStocktakeEventDetails;

using Abstractions.Messaging;
using Core.Models.Stocktake.Identifiers;

public sealed record GetStocktakeEventDetailsQuery(
    StocktakeEventId Id)
    : IQuery<StocktakeEventDetailsResponse>;