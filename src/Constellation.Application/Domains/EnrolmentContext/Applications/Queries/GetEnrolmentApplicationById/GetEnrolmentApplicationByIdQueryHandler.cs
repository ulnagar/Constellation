namespace Constellation.Application.Domains.EnrolmentContext.Applications.Queries.GetEnrolmentApplicationById;

using Abstractions.Messaging;
using Core.Models.EnrolmentContext.Application;
using Core.Models.EnrolmentContext.Application.Errors;
using Core.Models.EnrolmentContext.Application.Repositories;
using Core.Models.EnrolmentContext.EnrolmentPeriod;
using Core.Models.EnrolmentContext.EnrolmentPeriod.Repositories;
using Core.Shared;
using Models;
using Serilog;

internal sealed class GetEnrolmentApplicationByIdQueryHandler
: IQueryHandler<GetEnrolmentApplicationByIdQuery, EnrolmentApplicationResponse>
{
    private readonly IEnrolmentApplicationRepository _applicationRepository;
    private readonly IEnrolmentPeriodRepository _periodRepository;
    private readonly ILogger _logger;

    public GetEnrolmentApplicationByIdQueryHandler(
        IEnrolmentApplicationRepository applicationRepository,
        IEnrolmentPeriodRepository periodRepository,
        ILogger logger)
    {
        _applicationRepository = applicationRepository;
        _periodRepository = periodRepository;
        _logger = logger
            .ForContext<GetEnrolmentApplicationByIdQuery>();
    }

    public async Task<Result<EnrolmentApplicationResponse>> Handle(GetEnrolmentApplicationByIdQuery request, CancellationToken cancellationToken)
    {
        Application? application = await _applicationRepository.GetApplicationById(request.Id, cancellationToken);

        if (application is null)
        {
            _logger
                .ForContext(nameof(GetEnrolmentApplicationByIdQuery), request, true)
                .ForContext(nameof(Error), EnrolmentApplicationErrors.NotFound(request.Id), true)
                .Warning("Failed to retrieve Enrolment Application with Id");

            return Result.Failure<EnrolmentApplicationResponse>(EnrolmentApplicationErrors.NotFound(request.Id));
        }

        EnrolmentPeriod? period = await _periodRepository.GetEnrolmentPeriodById(application.PeriodId, cancellationToken);

        return new EnrolmentApplicationResponse(
            application.Id,
            application.PeriodId,
            period?.Label ?? string.Empty,
            period?.Year ?? string.Empty,
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
            application.SelectedCourses);
    }
}
