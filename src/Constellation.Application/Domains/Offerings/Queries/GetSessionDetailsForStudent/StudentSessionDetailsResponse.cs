namespace Constellation.Application.Domains.Offerings.Queries.GetSessionDetailsForStudent;

using System.Collections.Generic;

public sealed record StudentSessionDetailsResponse(
    string PeriodSortOrder,
    string PeriodName,
    string OfferingName,
    List<string> Teachers,
    int Duration);