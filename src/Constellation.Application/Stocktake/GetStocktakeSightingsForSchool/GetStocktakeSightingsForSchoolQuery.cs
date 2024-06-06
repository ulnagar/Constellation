namespace Constellation.Application.Stocktake.GetStocktakeSightingsForSchool;

using Constellation.Application.Abstractions.Messaging;
using Models;
using System;
using System.Collections.Generic;

public sealed record GetStocktakeSightingsForSchoolQuery(
    string SchoolCode,
    Guid StocktakeEventId)
    : IQuery<List<StocktakeSightingResponse>>;