namespace Constellation.Application.Periods.GetPeriodById;

using Abstractions.Messaging;

public sealed record GetPeriodByIdQuery(
    int PeriodId)
    : IQuery<PeriodResponse>;