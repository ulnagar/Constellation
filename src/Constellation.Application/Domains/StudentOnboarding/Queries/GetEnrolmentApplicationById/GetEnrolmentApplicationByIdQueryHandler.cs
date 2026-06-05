namespace Constellation.Application.Domains.StudentOnboarding.Queries.GetEnrolmentApplicationById;

using Abstractions.Messaging;
using Core.Models.StudentOnboarding;
using Core.Models.StudentOnboarding.Errors;
using Core.Models.StudentOnboarding.Repositories;
using Core.Shared;
using Models;
using Serilog;

internal sealed class GetEnrolmentApplicationByIdQueryHandler
: IQueryHandler<GetEnrolmentApplicationByIdQuery, EnrolmentApplicationResponse>
{
    private readonly IOnboardingRepository _onboardingRepository;
    private readonly ILogger _logger;

    public GetEnrolmentApplicationByIdQueryHandler(
        IOnboardingRepository onboardingRepository,
        ILogger logger)
    {
        _onboardingRepository = onboardingRepository;
        _logger = logger
            .ForContext<GetEnrolmentApplicationByIdQuery>();
    }

    public async Task<Result<EnrolmentApplicationResponse>> Handle(GetEnrolmentApplicationByIdQuery request, CancellationToken cancellationToken)
    {
        Application? application = await _onboardingRepository.GetApplicationById(request.ApplicationId, cancellationToken);

        if (application is null)
        {
            _logger
                .ForContext(nameof(GetEnrolmentApplicationByIdQuery), request, true)
                .ForContext(nameof(Error), ApplicationErrors.NotFoundById(request.ApplicationId), true)
                .Warning("Failed to retrieve Application");

            return Result.Failure<EnrolmentApplicationResponse>(ApplicationErrors.NotFoundById(request.ApplicationId));
        }

        return new EnrolmentApplicationResponse(
            application.Id,
            application.ApplicantId,
            application.Applicant.StudentReferenceNumber,
            application.Applicant.Name,
            application.Applicant.EmailAddress,
            application.Applicant.Gender,
            application.Applicant.IndigenousStatus,
            application.Program,
            application.Year,
            application.Grade,
            application.SchoolCode,
            application.SchoolName,
            application.State,
            application.Deadline);
    }
}
