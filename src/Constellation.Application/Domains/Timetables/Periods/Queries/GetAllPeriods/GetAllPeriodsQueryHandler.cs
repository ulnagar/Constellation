namespace Constellation.Application.Domains.Timetables.Periods.Queries.GetAllPeriods;

using Abstractions.Messaging;
using Core.Models.Timetables;
using Core.Models.Timetables.Repositories;
using Core.Shared;
using Serilog;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetAllPeriodsQueryHandler
    : IQueryHandler<GetAllPeriodsQuery, List<PeriodResponse>>
{
    private readonly IPeriodRepository _periodRepository;
    private readonly ILogger _logger;

    public GetAllPeriodsQueryHandler(
        IPeriodRepository periodRepository,
        ILogger logger)
    {
        _periodRepository = periodRepository;
        _logger = logger.ForContext<GetAllPeriodsQuery>();
    }

    public async Task<Result<List<PeriodResponse>>> Handle(GetAllPeriodsQuery request, CancellationToken cancellationToken)
    {
        List<Period> periods = await _periodRepository.GetAll(cancellationToken);

        List<PeriodResponse> response = new();

        foreach (Period period in periods)
        {
            response.Add(new(
                period.Id,
                period.Name,
                period.GroupName()));
        }

        return response;
    }
}
