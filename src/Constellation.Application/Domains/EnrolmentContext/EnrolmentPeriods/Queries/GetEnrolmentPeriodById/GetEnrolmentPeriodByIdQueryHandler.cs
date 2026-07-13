namespace Constellation.Application.Domains.EnrolmentContext.EnrolmentPeriods.Queries.GetEnrolmentPeriodById;

using Abstractions.Messaging;
using Constellation.Core.Models.EnrolmentContext.Application.Repositories;
using Constellation.Core.Models.EnrolmentContext.EnrolmentPeriod;
using Core.Abstractions.Clock;
using Core.Models.EnrolmentContext.EnrolmentPeriod.Errors;
using Core.Shared;
using Models;
using Serilog;

internal sealed class GetEnrolmentPeriodByIdQueryHandler
: IQueryHandler<GetEnrolmentPeriodByIdQuery, EnrolmentPeriodResponse>
{
    private readonly IEnrolmentApplicationRepository _repository;
    private readonly IDateTimeProvider _dateTime;
    private readonly ILogger _logger;

    public GetEnrolmentPeriodByIdQueryHandler(
        IEnrolmentApplicationRepository repository,
        IDateTimeProvider dateTime,
        ILogger logger)
    {
        _repository = repository;
        _dateTime = dateTime;
        _logger = logger
            .ForContext<GetEnrolmentPeriodByIdQuery>();
    }

    public async Task<Result<EnrolmentPeriodResponse>> Handle(GetEnrolmentPeriodByIdQuery request, CancellationToken cancellationToken)
    {
        EnrolmentPeriod? period = await _repository.GetEnrolmentPeriodById(request.Id, cancellationToken);

        if (period is null)
        {
            _logger
                .ForContext(nameof(GetEnrolmentPeriodByIdQuery), request, true)
                .ForContext(nameof(Error), EnrolmentPeriodErrors.NotFound(request.Id), true)
                .Warning("Failed to retrieve Enrolment Period");

            return Result.Failure<EnrolmentPeriodResponse>(EnrolmentPeriodErrors.NotFound(request.Id));
        }

        return new EnrolmentPeriodResponse(
            period.Id,
            period.Label,
            period.OpenAt,
            period.ClosedAt,
            period.GetStatus(_dateTime.Now),
            period.Program,
            period.IsSuspended,
            period.SuspensionReason);
    }
}
