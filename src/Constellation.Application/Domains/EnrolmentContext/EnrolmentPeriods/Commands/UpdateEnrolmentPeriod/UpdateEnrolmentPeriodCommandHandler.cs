namespace Constellation.Application.Domains.EnrolmentContext.EnrolmentPeriods.Commands.UpdateEnrolmentPeriod;

using Abstractions.Messaging;
using Core.Models.EnrolmentContext.EnrolmentPeriod;
using Core.Models.EnrolmentContext.EnrolmentPeriod.Errors;
using Core.Models.EnrolmentContext.EnrolmentPeriod.Repositories;
using Core.Shared;
using Interfaces;
using Serilog;

internal sealed class UpdateEnrolmentPeriodCommandHandler
: ICommandHandler<UpdateEnrolmentPeriodCommand>
{
    private readonly IEnrolmentPeriodRepository _periodRepository;
    private readonly IEnrolmentUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public UpdateEnrolmentPeriodCommandHandler(
        IEnrolmentPeriodRepository periodRepository,
        IEnrolmentUnitOfWork unitOfWork,
        ILogger logger)
    {
        _periodRepository = periodRepository;
        _unitOfWork = unitOfWork;
        _logger = logger
            .ForContext<UpdateEnrolmentPeriodCommand>();
    }

    public async Task<Result> Handle(UpdateEnrolmentPeriodCommand request, CancellationToken cancellationToken)
    {
        EnrolmentPeriod? period = await _periodRepository.GetEnrolmentPeriodById(request.Id, cancellationToken);

        if (period is null)
        {
            _logger
                .ForContext(nameof(UpdateEnrolmentPeriodCommand), request, true)
                .ForContext(nameof(Error), EnrolmentPeriodErrors.NotFound(request.Id), true)
                .Warning("Failed to update Enrolment Period");

            return Result.Failure(EnrolmentPeriodErrors.NotFound(request.Id));
        }

        Result update = period.Update(
            request.Label,
            request.OpenAt,
            request.ClosedAt,
            request.Program);

        if (update.IsFailure)
        {
            _logger
                .ForContext(nameof(UpdateEnrolmentPeriodCommand), request, true)
                .ForContext(nameof(Error), update.Error, true)
                .Warning("Failed to update Enrolment Period");

            return Result.Failure(update.Error);
        }

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
