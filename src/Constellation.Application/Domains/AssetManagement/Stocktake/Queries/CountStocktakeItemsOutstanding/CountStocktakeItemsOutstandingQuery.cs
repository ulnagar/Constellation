namespace Constellation.Application.Domains.AssetManagement.Stocktake.Queries.CountStocktakeItemsOutstanding;

using Abstractions.Messaging;
using Constellation.Core.Models.Stocktake.Identifiers;

public sealed record CountStocktakeItemsOutstandingQuery()
    : IQuery<(StocktakeEventId EventId, double Percentage)>;
