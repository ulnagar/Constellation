namespace Constellation.Application.Domains.StudentOnboarding.Queries.GetEnrolmentApplications;

using Abstractions.Messaging;
using Core.Models.StudentOnboarding;
using Core.Models.StudentOnboarding.Repositories;
using Core.Shared;
using Models;
using Serilog;
using System.Collections.Generic;

internal sealed class GetEnrolmentApplicationsQueryHandler
: IQueryHandler<GetEnrolmentApplicationsQuery, List<EnrolmentApplicationResponse>>
{
    private readonly IOnboardingRepository _onboardingRepository;
    private readonly ILogger _logger;

    public GetEnrolmentApplicationsQueryHandler(
        IOnboardingRepository onboardingRepository,
        ILogger logger)
    {
        _onboardingRepository = onboardingRepository;
        _logger = logger
            .ForContext<GetEnrolmentApplicationsQuery>();
    }

    public async Task<Result<List<EnrolmentApplicationResponse>>> Handle(GetEnrolmentApplicationsQuery request, CancellationToken cancellationToken)
    {
        List<EnrolmentApplicationResponse> responses = [];
        
        List<Application> applications = await _onboardingRepository.GetAllApplications(cancellationToken);

        foreach (var application in applications)
        {
            responses.Add(new(
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
                application.Deadline));
        }

        return responses;
    }
}
