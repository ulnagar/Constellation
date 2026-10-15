namespace Constellation.Application.Domains.EnrolmentContext.Applications.Queries.GetEnrolmentApplicationsByPeriod;

using Abstractions.Messaging;
using Core.Models.EnrolmentContext.Application;
using Core.Models.EnrolmentContext.Application.Repositories;
using Core.Models.EnrolmentContext.EnrolmentStudent;
using Core.Models.EnrolmentContext.EnrolmentStudent.Identifiers;
using Core.Models.EnrolmentContext.EnrolmentStudent.Repositories;
using Core.Shared;
using Serilog;
using System.Collections.Generic;

internal sealed class GetEnrolmentApplicationsByPeriodQueryHandler
: IQueryHandler<GetEnrolmentApplicationsByPeriodQuery, List<EnrolmentApplicationSummary>>
{
    private readonly IEnrolmentApplicationRepository _applicationRepository;
    private readonly IEnrolmentStudentRepository _studentRepository;
    private readonly ILogger _logger;

    public GetEnrolmentApplicationsByPeriodQueryHandler(
        IEnrolmentApplicationRepository applicationRepository,
        IEnrolmentStudentRepository studentRepository,
        ILogger logger)
    {
        _applicationRepository = applicationRepository;
        _studentRepository = studentRepository;
        _logger = logger
            .ForContext<GetEnrolmentApplicationsByPeriodQuery>();
    }

    public async Task<Result<List<EnrolmentApplicationSummary>>> Handle(GetEnrolmentApplicationsByPeriodQuery request, CancellationToken cancellationToken)
    {
        List<EnrolmentApplicationSummary> response = [];
        List<Application> applications = await _applicationRepository.GetApplicationsByPeriod(request.PeriodId, cancellationToken);

        if (applications.Count == 0)
            return response;

        List<EnrolmentStudentId> studentIds = applications.Select(entry => entry.StudentId).Distinct().ToList();

        List<EnrolmentStudent> students = await _studentRepository.GetListFromIds(studentIds, cancellationToken);
        
        foreach (Application application in applications)
        {
            EnrolmentStudent? student = students.FirstOrDefault(entry => entry.Id == application.StudentId);

            if (student is null)
                continue;

            response.Add(new(
                application.Id,
                student.Name,
                student.Gender,
                application.ApplicationReference ?? string.Empty,
                application.DestinationSchoolCode,
                application.DestinationSchool ?? string.Empty,
                application.Program,
                application.Grade,
                application.Status));
        }

        return response;
    }
}
