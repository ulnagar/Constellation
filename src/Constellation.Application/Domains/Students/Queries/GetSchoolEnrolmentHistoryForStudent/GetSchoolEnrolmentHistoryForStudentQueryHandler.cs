namespace Constellation.Application.Domains.Students.Queries.GetSchoolEnrolmentHistoryForStudent;

using Abstractions.Messaging;
using Core.Models.Students;
using Core.Models.Students.Errors;
using Core.Models.Students.Repositories;
using Core.Shared;
using Serilog;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetSchoolEnrolmentHistoryForStudentQueryHandler
: IQueryHandler<GetSchoolEnrolmentHistoryForStudentQuery, List<SchoolEnrolmentResponse>>
{
    private readonly IStudentRepository _studentRepository;
    private readonly ILogger _logger;

    public GetSchoolEnrolmentHistoryForStudentQueryHandler(
        IStudentRepository studentRepository,
        ILogger logger)
    {
        _studentRepository = studentRepository;
        _logger = logger
            .ForContext<GetSchoolEnrolmentHistoryForStudentQuery>();
    }
    
    public async Task<Result<List<SchoolEnrolmentResponse>>> Handle(GetSchoolEnrolmentHistoryForStudentQuery request, CancellationToken cancellationToken)
    {
        List<SchoolEnrolmentResponse> response = new();

        Student student = await _studentRepository.GetById(request.StudentId, cancellationToken);

        if (student is null)
        {
            _logger
                .ForContext(nameof(GetSchoolEnrolmentHistoryForStudentQuery), request, true)
                .ForContext(nameof(Error), StudentErrors.NotFound(request.StudentId), true)
                .Warning("Failed to retrieve Student School Enrolment records");

            return Result.Failure<List<SchoolEnrolmentResponse>>(StudentErrors.NotFound(request.StudentId));
        }

        foreach (SchoolEnrolment enrolment in student.SchoolEnrolments)
        {
            response.Add(new(
                enrolment.Id,
                enrolment.SchoolCode,
                enrolment.SchoolName,
                enrolment.Grade,
                enrolment.Year,
                enrolment.StartDate,
                enrolment.EndDate,
                enrolment.IsDeleted));
        }

        return response;
    }
}
