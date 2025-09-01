namespace Constellation.Application.Domains.AssetManagement.Stocktake.Queries.CountStocktakeItemsOutstanding;

using Abstractions.Messaging;

public sealed record CountStocktakeItemsOutstandingQuery()
    : IQuery<double>;
