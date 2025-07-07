namespace Constellation.Application.Domains.AssetManagement.Stocktake.Queries.GetStocktakeSightingsForSchool;

using Constellation.Application.Abstractions.Messaging;
using Core.Models.Stocktake.Identifiers;
using Models;
using System.Collections.Generic;

public sealed record GetStocktakeSightingsForSchoolQuery(
    string SchoolCode,
    StocktakeEventId StocktakeEventId)
    : IQuery<List<StocktakeSightingResponse>>;