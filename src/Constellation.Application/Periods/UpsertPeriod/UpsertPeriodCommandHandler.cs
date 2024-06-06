namespace Constellation.Application.Periods.UpsertPeriod;

using Abstractions.Messaging;
using Core.Errors;
using Core.Models;
using Core.Shared;
using Interfaces.Repositories;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

internal sealed class UpsertPeriodCommandHandler
: ICommandHandler<UpsertPeriodCommand>
{
    private readonly ITimetablePeriodRepository _periodRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public UpsertPeriodCommandHandler(
        ITimetablePeriodRepository periodRepository,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _periodRepository = periodRepository;
        _unitOfWork = unitOfWork;
        _logger = logger.ForContext<UpsertPeriodCommand>();
    }

    public async Task<Result> Handle(UpsertPeriodCommand request, CancellationToken cancellationToken)
    {
        TimetablePeriod period = request.Id is null
            ? new TimetablePeriod()
            : await _periodRepository.GetById(request.Id.Value, cancellationToken);

        if (period is null)
        {
            _logger
                .ForContext(nameof(UpsertPeriodCommand), request, true)
                .ForContext(nameof(Error), DomainErrors.Period.NotFound(request.Id.Value), true)
                .Warning("Failed to update Period");

            return Result.Failure(DomainErrors.Period.NotFound(request.Id.Value));
        }

        period.Day = request.Day;
        period.Timetable = request.Timetable;
        period.Period = request.Period;
        period.StartTime = request.StartTime;
        period.EndTime = request.EndTime;
        period.Name = request.Name;
        period.Type = request.Type;

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
