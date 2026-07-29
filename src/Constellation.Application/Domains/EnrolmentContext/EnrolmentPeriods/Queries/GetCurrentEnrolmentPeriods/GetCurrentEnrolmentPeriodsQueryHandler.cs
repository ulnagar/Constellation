namespace Constellation.Application.Domains.EnrolmentContext.EnrolmentPeriods.Queries.GetCurrentEnrolmentPeriods;

using Abstractions.Messaging;
using Core.Abstractions.Clock;
using Core.Models.EnrolmentContext.EnrolmentPeriod;
using Core.Models.EnrolmentContext.EnrolmentPeriod.Repositories;
using Core.Shared;
using Models;
using Serilog;
using System.Collections.Generic;

internal sealed class GetCurrentEnrolmentPeriodsQueryHandler
: IQueryHandler<GetCurrentEnrolmentPeriodsQuery, List<EnrolmentPeriodResponse>>
{
    private readonly IEnrolmentPeriodRepository _periodRepository;
    private readonly IDateTimeProvider _dateTime;
    private readonly ILogger _logger;

    public GetCurrentEnrolmentPeriodsQueryHandler(
        IEnrolmentPeriodRepository periodRepository,
        IDateTimeProvider dateTime,
        ILogger logger)
    {
        _periodRepository = periodRepository;
        _dateTime = dateTime;
        _logger = logger
            .ForContext<GetCurrentEnrolmentPeriodsQuery>();
    }

    public async Task<Result<List<EnrolmentPeriodResponse>>> Handle(GetCurrentEnrolmentPeriodsQuery request, CancellationToken cancellationToken)
    {
        List<EnrolmentPeriod> periods = await _periodRepository.GetCurrentEnrolmentPeriods(cancellationToken);

        List<EnrolmentPeriodResponse> response = [];

        foreach (EnrolmentPeriod period in periods)
        {
            response.Add(new(
                period.Id,
                period.Label,
                period.Year,
                period.OpenAt,
                period.ClosedAt,
                period.GetStatus(_dateTime.Now),
                period.Program,
                period.AvailableCourses,
                period.IsSuspended,
                period.SuspensionReason));
        }

        return response;
    }
}
