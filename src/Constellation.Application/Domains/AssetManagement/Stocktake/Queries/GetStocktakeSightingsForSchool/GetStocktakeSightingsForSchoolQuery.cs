namespace Constellation.Application.Domains.AssetManagement.Stocktake.Queries.GetStocktakeSightingsForSchool;

using Constellation.Application.Abstractions.Messaging;
using Core.Models.Identifiers;
using Core.Models.Stocktake.Identifiers;
using Models;
using System.Collections.Generic;

public sealed record GetStocktakeSightingsForSchoolQuery(
    SchoolCode SchoolCode,
    StocktakeEventId StocktakeEventId)
    : IQuery<List<StocktakeSightingResponse>>;