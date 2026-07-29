namespace Constellation.Application.Domains.EnrolmentContext.Applications.Queries.GetEnrolmentApplicationsByPeriod;

using Abstractions.Messaging;
using Core.Models.EnrolmentContext.Application;
using Core.Models.EnrolmentContext.Application.Repositories;
using Core.Shared;
using Models;
using Serilog;
using System.Collections.Generic;

internal sealed class GetEnrolmentApplicationsByPeriodQueryHandler
: IQueryHandler<GetEnrolmentApplicationsByPeriodQuery, List<EnrolmentApplicationResponse>>
{
    private readonly IEnrolmentApplicationRepository _applicationRepository;
    private readonly ILogger _logger;

    public GetEnrolmentApplicationsByPeriodQueryHandler(
        IEnrolmentApplicationRepository applicationRepository,
        ILogger logger)
    {
        _applicationRepository = applicationRepository;
        _logger = logger
            .ForContext<GetEnrolmentApplicationsByPeriodQuery>();
    }

    public async Task<Result<List<EnrolmentApplicationResponse>>> Handle(GetEnrolmentApplicationsByPeriodQuery request, CancellationToken cancellationToken)
    {
        List<EnrolmentApplicationResponse> response = [];
        List<Application> applications = await _applicationRepository.GetApplicationsByPeriod(request.PeriodId, cancellationToken);

        if (applications.Count == 0)
            return response;
        
        foreach (Application application in applications)
        {
            response.Add(new(
                application.Id,
                application.PeriodId,
                string.Empty,
                string.Empty,
                application.StudentReferenceNumber,
                application.StudentName,
                application.StudentGender,
                application.DateOfBirth,
                application.StudentEmailAddress,
                application.ParentName,
                application.ParentEmailAddress,
                application.ParentPhoneNumber,
                application.MailingAddress,
                application.ApplicationReference ?? string.Empty,
                application.CurrentSchoolCode,
                application.CurrentSchool ?? string.Empty,
                application.DestinationSchoolCode,
                application.DestinationSchool ?? string.Empty,
                application.Program,
                application.Grade,
                application.Status,
                application.SelectedCourses));
        }

        return response;
    }
}
