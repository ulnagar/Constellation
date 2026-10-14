namespace Constellation.Application.Domains.EnrolmentContext.Applications.Queries.GetEnrolmentApplicationById;

using Abstractions.Messaging;
using Core.Models.EnrolmentContext.Application;
using Core.Models.EnrolmentContext.Application.Errors;
using Core.Models.EnrolmentContext.Application.Repositories;
using Core.Models.EnrolmentContext.EnrolmentParent;
using Core.Models.EnrolmentContext.EnrolmentPeriod;
using Core.Models.EnrolmentContext.EnrolmentPeriod.Repositories;
using Core.Models.EnrolmentContext.EnrolmentStudent;
using Core.Models.EnrolmentContext.EnrolmentStudent.Errors;
using Core.Models.EnrolmentContext.EnrolmentStudent.Repositories;
using Core.Shared;
using Models;
using Serilog;

internal sealed class GetEnrolmentApplicationByIdQueryHandler
: IQueryHandler<GetEnrolmentApplicationByIdQuery, EnrolmentApplicationResponse>
{
    private readonly IEnrolmentApplicationRepository _applicationRepository;
    private readonly IEnrolmentPeriodRepository _periodRepository;
    private readonly IEnrolmentStudentRepository _studentRepository;
    private readonly ILogger _logger;

    public GetEnrolmentApplicationByIdQueryHandler(
        IEnrolmentApplicationRepository applicationRepository,
        IEnrolmentPeriodRepository periodRepository,
        IEnrolmentStudentRepository studentRepository,
        ILogger logger)
    {
        _applicationRepository = applicationRepository;
        _periodRepository = periodRepository;
        _studentRepository = studentRepository;
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

        EnrolmentStudent? student = await _studentRepository.GetById(application.StudentId, cancellationToken);

        if (student is null)
        {
            _logger
                .ForContext(nameof(GetEnrolmentApplicationByIdQuery), request, true)
                .ForContext(nameof(Error), EnrolmentStudentErrors.NotFound(application.StudentId), true)
                .Warning("Failed to retrieve Enrolment Application with Id");

            return Result.Failure<EnrolmentApplicationResponse>(EnrolmentStudentErrors.NotFound(application.StudentId));
        }
        
        EnrolmentPeriod? period = await _periodRepository.GetEnrolmentPeriodById(application.PeriodId, cancellationToken);

        List<EnrolmentApplicationResponse.Parent> parents = [];

        foreach (EnrolmentParent parent in application.Parents)
        {
            parents.Add(new(
                parent.Id,
                parent.Name,
                parent.Email,
                parent.PhoneNumber,
                parent.MailingAddress));
        }

        return new EnrolmentApplicationResponse(
            application.Id,
            application.PeriodId,
            period?.Label ?? string.Empty,
            period?.Year ?? string.Empty,
            student.Id,
            student.StudentReferenceNumber,
            student.Name, 
            student.StudentGender, 
            student.DateOfBirth, 
            student.StudentEmailAddress,
            parents,
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
