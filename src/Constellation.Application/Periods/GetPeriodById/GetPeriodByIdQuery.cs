namespace Constellation.Application.Periods.GetPeriodById;

using Abstractions.Messaging;
using Core.Models.Timetables.Identifiers;

public sealed record GetPeriodByIdQuery(
    PeriodId PeriodId)
    : IQuery<PeriodResponse>;