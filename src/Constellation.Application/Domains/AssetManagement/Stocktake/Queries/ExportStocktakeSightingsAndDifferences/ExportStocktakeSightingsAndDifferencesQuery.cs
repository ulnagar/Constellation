namespace Constellation.Application.Domains.AssetManagement.Stocktake.Queries.ExportStocktakeSightingsAndDifferences;

using Abstractions.Messaging;
using Core.Models.Stocktake.Identifiers;
using DTOs;

public sealed record ExportStocktakeSightingsAndDifferencesQuery(
    StocktakeEventId EventId)
    : IQuery<FileDto>;
