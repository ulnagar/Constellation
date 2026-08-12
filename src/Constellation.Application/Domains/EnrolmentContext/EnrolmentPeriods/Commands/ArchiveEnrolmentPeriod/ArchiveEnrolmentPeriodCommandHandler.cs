namespace Constellation.Application.Domains.EnrolmentContext.EnrolmentPeriods.Commands.ArchiveEnrolmentPeriod;

using Abstractions.Messaging;
using Core.Models.EnrolmentContext.EnrolmentPeriod;
using Core.Models.EnrolmentContext.EnrolmentPeriod.Errors;
using Core.Models.EnrolmentContext.EnrolmentPeriod.Repositories;
using Core.Shared;
using Interfaces;
using Serilog;

internal sealed class ArchiveEnrolmentPeriodCommandHandler
: ICommandHandler<ArchiveEnrolmentPeriodCommand>
{
    private readonly IEnrolmentPeriodRepository _periodRepository;
    private readonly IEnrolmentUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public ArchiveEnrolmentPeriodCommandHandler(
        IEnrolmentPeriodRepository periodRepository,
        IEnrolmentUnitOfWork unitOfWork,
        ILogger logger)
    {
        _periodRepository = periodRepository;
        _unitOfWork = unitOfWork;
        _logger = logger
            .ForContext<ArchiveEnrolmentPeriodCommand>();
    }
    public async Task<Result> Handle(ArchiveEnrolmentPeriodCommand request, CancellationToken cancellationToken)
    {
        EnrolmentPeriod? period = await _periodRepository.GetEnrolmentPeriodById(request.PeriodId, cancellationToken);

        if (period is null)
        {
            _logger
                .ForContext(nameof(ArchiveEnrolmentPeriodCommand), request, true)
                .ForContext(nameof(Error), EnrolmentPeriodErrors.NotFound(request.PeriodId), true)
                .Warning("Failed to mark Enrolment Period as archived");

            return Result.Failure(EnrolmentPeriodErrors.NotFound(request.PeriodId));
        }

        period.Archive();
        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
