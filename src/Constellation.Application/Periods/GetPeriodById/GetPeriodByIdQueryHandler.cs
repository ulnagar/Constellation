namespace Constellation.Application.Periods.GetPeriodById;

using Abstractions.Messaging;
using Core.Errors;
using Core.Models;
using Core.Shared;
using Interfaces.Repositories;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetPeriodByIdQueryHandler
: IQueryHandler<GetPeriodByIdQuery, PeriodResponse>
{
    private readonly ITimetablePeriodRepository _periodRepository;
    private readonly ILogger _logger;

    public GetPeriodByIdQueryHandler(
        ITimetablePeriodRepository periodRepository,
        ILogger logger)
    {
        _periodRepository = periodRepository;
        _logger = logger.ForContext<GetPeriodByIdQuery>();
    }

    public async Task<Result<PeriodResponse>> Handle(GetPeriodByIdQuery request, CancellationToken cancellationToken)
    {
        TimetablePeriod? period = await _periodRepository.GetById(request.PeriodId, cancellationToken);

        if (period is null)
        {
            _logger
                .ForContext(nameof(GetPeriodByIdQuery), request, true)
                .ForContext(nameof(Error), DomainErrors.Period.NotFound(request.PeriodId), true)
                .Warning("Failed to retrieve Period");

            return Result.Failure<PeriodResponse>(DomainErrors.Period.NotFound(request.PeriodId));
        }

        return new PeriodResponse(
            period.Id,
            period.Day,
            period.Period,
            period.Timetable,
            period.StartTime,
            period.EndTime,
            period.Name,
            period.Type);
    }
}
