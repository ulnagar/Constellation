namespace Constellation.Application.Domains.Timetables.Periods.Commands.UpsertPeriod;

using Abstractions.Messaging;
using Core.Models.Timetables;
using Core.Models.Timetables.Errors;
using Core.Models.Timetables.Identifiers;
using Core.Models.Timetables.Repositories;
using Core.Shared;
using Interfaces.Repositories;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

internal sealed class UpsertPeriodCommandHandler
: ICommandHandler<UpsertPeriodCommand>
{
    private readonly IPeriodRepository _periodRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public UpsertPeriodCommandHandler(
        IPeriodRepository periodRepository,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _periodRepository = periodRepository;
        _unitOfWork = unitOfWork;
        _logger = logger.ForContext<UpsertPeriodCommand>();
    }

    public async Task<Result> Handle(UpsertPeriodCommand request, CancellationToken cancellationToken)
    {
        if (request.Id == PeriodId.Empty)
        {
            Period newPeriod = Period.Create(
                request.Timetable,
                request.Week,
                request.Day,
                request.PeriodCode,
                request.Name,
                request.Type,
                request.StartTime,
                request.EndTime);

            _periodRepository.Insert(newPeriod);
        }
        else
        {
            Period period = await _periodRepository.GetById(request.Id, cancellationToken);

            if (period is null)
            {
                _logger
                    .ForContext(nameof(UpsertPeriodCommand), request, true)
                    .ForContext(nameof(Error), PeriodErrors.NotFound(request.Id), true)
                    .Warning("Failed to update Period");

                return Result.Failure(PeriodErrors.NotFound(request.Id));
            }

            period.Update(
                request.Timetable,
                request.Week,
                request.Day,
                request.PeriodCode,
                request.Name,
                request.Type,
                request.StartTime,
                request.EndTime);
        }
        
        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
