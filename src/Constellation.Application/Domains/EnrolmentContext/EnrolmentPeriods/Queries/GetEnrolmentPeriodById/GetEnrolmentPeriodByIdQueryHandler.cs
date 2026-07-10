namespace Constellation.Application.Domains.EnrolmentContext.EnrolmentPeriods.Queries.GetEnrolmentPeriodById;

using Abstractions.Messaging;
using Constellation.Core.Models.EnrolmentContext.Application.Repositories;
using Constellation.Core.Models.EnrolmentContext.EnrolmentPeriod;
using Core.Models.EnrolmentContext.EnrolmentPeriod.Errors;
using Core.Shared;
using Models;
using Serilog;

internal sealed class GetEnrolmentPeriodByIdQueryHandler
: IQueryHandler<GetEnrolmentPeriodByIdQuery, EnrolmentPeriodResponse>
{
    private readonly IEnrolmentApplicationRepository _repository;
    private readonly ILogger _logger;

    public GetEnrolmentPeriodByIdQueryHandler(
        IEnrolmentApplicationRepository repository,
        ILogger logger)
    {
        _repository = repository;
        _logger = logger
            .ForContext<GetEnrolmentPeriodByIdQuery>();
    }

    public async Task<Result<EnrolmentPeriodResponse>> Handle(GetEnrolmentPeriodByIdQuery request, CancellationToken cancellationToken)
    {
        EnrolmentPeriod? period = await _repository.GetEnrolmentPeriodById(request.Id, cancellationToken);

        if (period is null)
            return Result.Failure<EnrolmentPeriodResponse>(EnrolmentPeriodErrors.NotFound(request.Id));

        return new EnrolmentPeriodResponse(
            period.Id,
            period.Label,
            period.OpenAt,
            period.ClosedAt,
            period.Status,
            period.Program);
    }
}
