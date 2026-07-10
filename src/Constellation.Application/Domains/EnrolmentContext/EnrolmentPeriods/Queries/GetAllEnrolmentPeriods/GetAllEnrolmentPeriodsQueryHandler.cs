namespace Constellation.Application.Domains.EnrolmentContext.EnrolmentPeriods.Queries.GetAllEnrolmentPeriods;

using Abstractions.Messaging;
using Constellation.Core.Models.EnrolmentContext.EnrolmentPeriod;
using Core.Models.EnrolmentContext.Application.Repositories;
using Core.Shared;
using Models;
using Serilog;
using System.Collections.Generic;

internal sealed class GetAllEnrolmentPeriodsQueryHandler
: IQueryHandler<GetAllEnrolmentPeriodsQuery, List<EnrolmentPeriodResponse>>
{
    private readonly IEnrolmentApplicationRepository _repository;
    private readonly ILogger _logger;

    public GetAllEnrolmentPeriodsQueryHandler(
        IEnrolmentApplicationRepository repository,
        ILogger logger)
    {
        _repository = repository;
        _logger = logger
            .ForContext<GetAllEnrolmentPeriodsQuery>();
    }

    public async Task<Result<List<EnrolmentPeriodResponse>>> Handle(GetAllEnrolmentPeriodsQuery request, CancellationToken cancellationToken)
    {
        List<EnrolmentPeriod> periods = await _repository.GetAllEnrolmentPeriods(cancellationToken);

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
