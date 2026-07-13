namespace Constellation.Application.Domains.EnrolmentContext.EnrolmentPeriods.Commands.CreateEnrolmentPeriod;

using Abstractions.Messaging;
using Core.Models.EnrolmentContext.Application.Repositories;
using Core.Models.EnrolmentContext.EnrolmentPeriod;
using Core.Shared;
using Interfaces;
using Serilog;

internal sealed class CreateEnrolmentPeriodCommandHandler
: ICommandHandler<CreateEnrolmentPeriodCommand>
{
    private readonly IEnrolmentApplicationRepository _repository;
    private readonly IEnrolmentUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public CreateEnrolmentPeriodCommandHandler(
        IEnrolmentApplicationRepository repository,
        IEnrolmentUnitOfWork unitOfWork,
        ILogger logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _logger = logger
            .ForContext<CreateEnrolmentPeriodCommand>();
    }

    public async Task<Result> Handle(CreateEnrolmentPeriodCommand request, CancellationToken cancellationToken)
    {
        Result<EnrolmentPeriod> period = EnrolmentPeriod.Create(
            request.Label,
            request.OpenAt,
            request.ClosedAt,
            request.Program);

        if (period.IsFailure)
        {
            _logger
                .ForContext(nameof(CreateEnrolmentPeriodCommand), request, true)
                .ForContext(nameof(Error), period.Error, true)
                .Warning("Failed to create new Enrolment Period");

            return period;
        }

        _repository.Insert(period.Value);
        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
