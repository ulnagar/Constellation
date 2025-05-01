namespace Constellation.Application.Domains.ClassCovers.Queries.GetAllCoversForCalendarYear;

using Abstractions.Messaging;
using Models;
using System.Collections.Generic;

public sealed record GetAllCoversForCalendarYearQuery
    : IQuery<List<CoversListResponse>>;