namespace Constellation.Application.Domains.EnrolmentContext.EnrolmentPeriods.Commands.SuspendEnrolmentPeriod;

using Abstractions.Messaging;
using Constellation.Core.Models.EnrolmentContext.EnrolmentPeriod;
using Constellation.Core.Models.EnrolmentContext.EnrolmentPeriod.Errors;
using Core.Models.EnrolmentContext.EnrolmentPeriod.Repositories;
using Core.Shared;
using Interfaces;
using Serilog;

internal sealed class SuspendEnrolmentPeriodCommandHandler
    : ICommandHandler<SuspendEnrolmentPeriodCommand>
{
    private readonly IEnrolmentPeriodRepository _periodRepository;
    private readonly IEnrolmentUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public SuspendEnrolmentPeriodCommandHandler(
        IEnrolmentPeriodRepository periodRepository,
        IEnrolmentUnitOfWork unitOfWork,
        ILogger logger)
    {
        _periodRepository = periodRepository;
        _unitOfWork = unitOfWork;
        _logger = logger
            .ForContext<SuspendEnrolmentPeriodCommand>();
    }

    public async Task<Result> Handle(SuspendEnrolmentPeriodCommand request, CancellationToken cancellationToken)
    {
        EnrolmentPeriod? period = await _periodRepository.GetEnrolmentPeriodById(request.PeriodId, cancellationToken);

        if (period is null)
        {
            _logger
                .ForContext(nameof(SuspendEnrolmentPeriodCommand), request, true)
                .ForContext(nameof(Error), EnrolmentPeriodErrors.NotFound(request.PeriodId), true)
                .Warning("Failed to suspend Enrolment Period");

            return Result.Failure(EnrolmentPeriodErrors.NotFound(request.PeriodId));
        }

        Result update = period.Suspend(request.SuspensionComment);

        if (update.IsFailure)
        {
            _logger
                .ForContext(nameof(SuspendEnrolmentPeriodCommand), request, true)
                .ForContext(nameof(Error), update.Error, true)
                .Warning("Failed to suspend Enrolment Period");

            return Result.Failure(update.Error);
        }

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
