namespace Constellation.Application.Domains.EnrolmentContext.EnrolmentPeriods.Queries.GetAllEnrolmentPeriods;

using Abstractions.Messaging;
using Constellation.Core.Models.EnrolmentContext.EnrolmentPeriod;
using Core.Abstractions.Clock;
using Core.Models.EnrolmentContext.EnrolmentPeriod.Repositories;
using Core.Shared;
using Models;
using Serilog;
using System.Collections.Generic;

internal sealed class GetAllEnrolmentPeriodsQueryHandler
: IQueryHandler<GetAllEnrolmentPeriodsQuery, List<EnrolmentPeriodResponse>>
{
    private readonly IEnrolmentPeriodRepository _periodRepository;
    private readonly IDateTimeProvider _dateTime;
    private readonly ILogger _logger;

    public GetAllEnrolmentPeriodsQueryHandler(
        IEnrolmentPeriodRepository periodRepository,
        IDateTimeProvider dateTime,
        ILogger logger)
    {
        _periodRepository = periodRepository;
        _dateTime = dateTime;
        _logger = logger
            .ForContext<GetAllEnrolmentPeriodsQuery>();
    }

    public async Task<Result<List<EnrolmentPeriodResponse>>> Handle(GetAllEnrolmentPeriodsQuery request, CancellationToken cancellationToken)
    {
        List<EnrolmentPeriod> periods = await _periodRepository.GetAllEnrolmentPeriods(cancellationToken);

        List<EnrolmentPeriodResponse> response = [];

        foreach (EnrolmentPeriod period in periods)
        {
            response.Add(new(
                period.Id,
                period.Label,
                period.OpenAt,
                period.ClosedAt,
                period.GetStatus(_dateTime.Now),
                period.Program,
                period.IsSuspended,
                period.SuspensionReason));
        }

        return response;
    }
}
