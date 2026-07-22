namespace Constellation.Application.Domains.EnrolmentContext.Applications.Queries.GetEnrolmentApplicationById;

using Abstractions.Messaging;
using Core.Models.EnrolmentContext.Application;
using Core.Models.EnrolmentContext.Application.Errors;
using Core.Models.EnrolmentContext.Application.Repositories;
using Core.Shared;
using Models;
using Serilog;

internal sealed class GetEnrolmentApplicationByIdQueryHandler
: IQueryHandler<GetEnrolmentApplicationByIdQuery, EnrolmentApplicationResponse>
{
    private readonly IEnrolmentApplicationRepository _applicationRepository;
    private readonly ILogger _logger;

    public GetEnrolmentApplicationByIdQueryHandler(
        IEnrolmentApplicationRepository applicationRepository,
        ILogger logger)
    {
        _applicationRepository = applicationRepository;
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

        return new EnrolmentApplicationResponse(
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
            application.Grade,
            application.Status);
    }
}
