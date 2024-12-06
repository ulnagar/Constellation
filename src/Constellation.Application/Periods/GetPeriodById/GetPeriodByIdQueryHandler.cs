namespace Constellation.Application.Periods.GetPeriodById;

using Abstractions.Messaging;
using Core.Models.Timetables;
using Core.Models.Timetables.Errors;
using Core.Models.Timetables.Repositories;
using Core.Shared;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetPeriodByIdQueryHandler
: IQueryHandler<GetPeriodByIdQuery, PeriodResponse>
{
    private readonly IPeriodRepository _periodRepository;
    private readonly ILogger _logger;

    public GetPeriodByIdQueryHandler(
        IPeriodRepository periodRepository,
        ILogger logger)
    {
        _periodRepository = periodRepository;
        _logger = logger.ForContext<GetPeriodByIdQuery>();
    }

    public async Task<Result<PeriodResponse>> Handle(GetPeriodByIdQuery request, CancellationToken cancellationToken)
    {
        Period? period = await _periodRepository.GetById(request.PeriodId, cancellationToken);

        if (period is null)
        {
            _logger
                .ForContext(nameof(GetPeriodByIdQuery), request, true)
                .ForContext(nameof(Error), PeriodErrors.NotFound(request.PeriodId), true)
                .Warning("Failed to retrieve Period");

            return Result.Failure<PeriodResponse>(PeriodErrors.NotFound(request.PeriodId));
        }

        return new PeriodResponse(
            period.Id,
            period.DayNumber,
            period.DaySequence,
            period.Timetable,
            period.StartTime,
            period.EndTime,
            period.Name,
            period.Type);
    }
}
