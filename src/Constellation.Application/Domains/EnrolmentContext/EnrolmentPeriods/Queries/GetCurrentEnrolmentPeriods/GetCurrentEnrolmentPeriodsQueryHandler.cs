namespace Constellation.Application.Domains.EnrolmentContext.EnrolmentPeriods.Queries.GetCurrentEnrolmentPeriods;

using Abstractions.Messaging;
using Core.Models.EnrolmentContext.Application.Repositories;
using Core.Models.EnrolmentContext.EnrolmentPeriod;
using Core.Shared;
using Models;
using Serilog;
using System.Collections.Generic;

internal sealed class GetCurrentEnrolmentPeriodsQueryHandler
: IQueryHandler<GetCurrentEnrolmentPeriodsQuery, List<EnrolmentPeriodResponse>>
{
    private readonly IEnrolmentApplicationRepository _repository;
    private readonly ILogger _logger;

    public GetCurrentEnrolmentPeriodsQueryHandler(
        IEnrolmentApplicationRepository repository,
        ILogger logger)
    {
        _repository = repository;
        _logger = logger
            .ForContext<GetCurrentEnrolmentPeriodsQuery>();
    }

    public async Task<Result<List<EnrolmentPeriodResponse>>> Handle(GetCurrentEnrolmentPeriodsQuery request, CancellationToken cancellationToken)
    {
        List<EnrolmentPeriod> periods = await _repository.GetCurrentEnrolmentPeriods(cancellationToken);

        List<EnrolmentPeriodResponse> response = [];

        foreach (EnrolmentPeriod period in periods)
        {
            response.Add(new(
                period.Id,
                period.Label,
                period.OpenAt,
                period.ClosedAt,
                period.Status,
                period.Program));
        }

        return response;
    }
}
