namespace Constellation.Application.Periods.GetAllPeriods;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Models;
using Constellation.Core.Shared;
using Serilog;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetAllPeriodsQueryHandler
    : IQueryHandler<GetAllPeriodsQuery, List<PeriodResponse>>
{
    private readonly ITimetablePeriodRepository _periodRepository;
    private readonly ILogger _logger;

    public GetAllPeriodsQueryHandler(
        ITimetablePeriodRepository periodRepository,
        ILogger logger)
    {
        _periodRepository = periodRepository;
        _logger = logger.ForContext<GetAllPeriodsQuery>();
    }

    public async Task<Result<List<PeriodResponse>>> Handle(GetAllPeriodsQuery request, CancellationToken cancellationToken)
    {
        List<TimetablePeriod> periods = await _periodRepository.GetAll(cancellationToken);

        List<PeriodResponse> response = new();

        foreach (TimetablePeriod period in periods)
        {
            response.Add(new(
                period.Id,
                period.Name,
                period.GroupName()));
        }

        return response;
    }
}
