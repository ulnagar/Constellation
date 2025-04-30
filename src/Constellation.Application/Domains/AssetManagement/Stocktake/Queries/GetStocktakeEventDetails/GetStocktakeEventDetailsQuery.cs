namespace Constellation.Application.Domains.AssetManagement.Stocktake.Queries.GetStocktakeEventDetails;

using Abstractions.Messaging;
using System;

public sealed record GetStocktakeEventDetailsQuery(
    Guid Id)
    : IQuery<StocktakeEventDetailsResponse>;