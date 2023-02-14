namespace Constellation.Application.ClassCovers.GetAllCoversForCalendarYear;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.ClassCovers.Models;
using System.Collections.Generic;

public sealed record GetAllCoversForCalendarYearQuery
    : IQuery<List<CoversListResponse>>;