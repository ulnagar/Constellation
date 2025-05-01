namespace Constellation.Application.Domains.Timetables.Periods.Queries.GetPeriodById;

using Abstractions.Messaging;
using Core.Models.Timetables.Identifiers;

public sealed record GetPeriodByIdQuery(
    PeriodId PeriodId)
    : IQuery<PeriodResponse>;