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
    private readonly IEnrolmentApplicationRepository _repository;
    private readonly ILogger _logger;

    public GetEnrolmentApplicationsByPeriodQueryHandler(
        IEnrolmentApplicationRepository repository,
        ILogger logger)
    {
        _repository = repository;
        _logger = logger
            .ForContext<GetEnrolmentApplicationsByPeriodQuery>();
    }

    public async Task<Result<List<EnrolmentApplicationResponse>>> Handle(GetEnrolmentApplicationsByPeriodQuery request, CancellationToken cancellationToken)
    {
        List<Application> applications = await _repository.GetApplicationsByPeriod(request.PeriodId, cancellationToken);

        List<EnrolmentApplicationResponse> response = [];

        foreach (Application application in applications)
        {
            response.Add(new(
                application.Id,
                application.PeriodId,
                application.StudentReferenceNumber,
                application.StudentName,
                application.StudentGender,
                application.DateOfBirth,
                application.StudentEmailAddress,
                application.ParentName,
                application.ParentEmailAddress,
                application.ParentPhoneNumber,
                application.MailingAddress,
                application.ApplicationReference,
                application.CurrentSchoolCode,
                application.CurrentSchool,
                application.DestinationSchoolCode,
                application.DestinationSchool,
                application.Program,
                application.Grade));
        }

        return response;
    }
}
