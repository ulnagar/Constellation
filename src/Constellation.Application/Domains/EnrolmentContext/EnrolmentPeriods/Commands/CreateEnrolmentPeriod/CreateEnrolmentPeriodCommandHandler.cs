namespace Constellation.Application.Domains.EnrolmentContext.EnrolmentPeriods.Commands.CreateEnrolmentPeriod;

using Abstractions.Messaging;
using Core.Models.EnrolmentContext.Application.Enums;
using Core.Models.EnrolmentContext.EnrolmentPeriod;
using Core.Models.EnrolmentContext.EnrolmentPeriod.Repositories;
using Core.Shared;
using Interfaces;
using Serilog;

internal sealed class CreateEnrolmentPeriodCommandHandler
: ICommandHandler<CreateEnrolmentPeriodCommand>
{
    private readonly IEnrolmentPeriodRepository _periodRepository;
    private readonly IEnrolmentUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public CreateEnrolmentPeriodCommandHandler(
        IEnrolmentPeriodRepository periodRepository,
        IEnrolmentUnitOfWork unitOfWork,
        ILogger logger)
    {
        _periodRepository = periodRepository;
        _unitOfWork = unitOfWork;
        _logger = logger
            .ForContext<CreateEnrolmentPeriodCommand>();
    }

    public async Task<Result> Handle(CreateEnrolmentPeriodCommand request, CancellationToken cancellationToken)
    {
        Result<EnrolmentPeriod> period = EnrolmentPeriod.Create(
            request.Label,
            request.Year,
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

        foreach (EnrolmentCourse course in request.AvailableCourses)
            period.Value.AddCourse(course);

        _periodRepository.Insert(period.Value);
        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
