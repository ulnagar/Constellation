namespace Constellation.Application.Domains.LinkedSystems.Sentral.Queries.GetTermsAndWeeksForCurrentYear;

using Abstractions.Messaging;
using System.Collections.Generic;

public sealed record GetTermsAndWeeksForCurrentYearQuery()
    : IQuery<List<SchoolCalendarWeek>>;