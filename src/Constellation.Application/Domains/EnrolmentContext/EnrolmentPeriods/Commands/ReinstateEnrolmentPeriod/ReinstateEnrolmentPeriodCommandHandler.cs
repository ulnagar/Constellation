namespace Constellation.Application.Domains.EnrolmentContext.EnrolmentPeriods.Commands.ReinstateEnrolmentPeriod;

using Abstractions.Messaging;
using Constellation.Application.Domains.EnrolmentContext.Interfaces;
using Constellation.Core.Models.EnrolmentContext.Application.Repositories;
using Constellation.Core.Models.EnrolmentContext.EnrolmentPeriod;
using Constellation.Core.Models.EnrolmentContext.EnrolmentPeriod.Errors;
using Core.Shared;
using Serilog;

internal sealed class ReinstateEnrolmentPeriodCommandHandler
: ICommandHandler<ReinstateEnrolmentPeriodCommand>
{
    private readonly IEnrolmentApplicationRepository _repository;
    private readonly IEnrolmentUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public ReinstateEnrolmentPeriodCommandHandler(
        IEnrolmentApplicationRepository repository,
        IEnrolmentUnitOfWork unitOfWork,
        ILogger logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _logger = logger
            .ForContext<ReinstateEnrolmentPeriodCommand>();
    }

    public async Task<Result> Handle(ReinstateEnrolmentPeriodCommand request, CancellationToken cancellationToken)
    {
        EnrolmentPeriod? period = await _repository.GetEnrolmentPeriodById(request.PeriodId, cancellationToken);

        if (period is null)
        {
            _logger
                .ForContext(nameof(ReinstateEnrolmentPeriodCommand), request, true)
                .ForContext(nameof(Error), EnrolmentPeriodErrors.NotFound(request.PeriodId), true)
                .Warning("Failed to reinstate Enrolment Period");

            return Result.Failure(EnrolmentPeriodErrors.NotFound(request.PeriodId));
        }

        Result update = period.Resume();

        if (update.IsFailure)
        {
            _logger
                .ForContext(nameof(ReinstateEnrolmentPeriodCommand), request, true)
                .ForContext(nameof(Error), update.Error, true)
                .Warning("Failed to reinstate Enrolment Period");

            return Result.Failure(update.Error);
        }

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
