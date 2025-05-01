namespace Constellation.Application.Domains.Students.Queries.GetStudentsFromSchoolForSelectionList;

using Abstractions.Messaging;
using Constellation.Core.Models.Students.Repositories;
using Core.Extensions;
using Core.Models.Students;
using Core.Shared;
using Serilog;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetStudentsFromSchoolForSelectionQueryHandler 
    : IQueryHandler<GetStudentsFromSchoolForSelectionQuery, List<StudentSelectionResponse>>
{
    private readonly IStudentRepository _studentRepository;
    private readonly ILogger _logger;

    public GetStudentsFromSchoolForSelectionQueryHandler(
        IStudentRepository studentRepository,
        ILogger logger)
    {
        _studentRepository = studentRepository;
        _logger = logger.ForContext<GetStudentsFromSchoolForSelectionQuery>();
    }

    public async Task<Result<List<StudentSelectionResponse>>> Handle(GetStudentsFromSchoolForSelectionQuery request, CancellationToken cancellationToken)
    {
        List<StudentSelectionResponse> response = new();

        List<Student> students = await _studentRepository.GetCurrentStudentsFromSchool(request.SchoolCode, cancellationToken);

        foreach (Student student in students)
        {
            SchoolEnrolment enrolment = student.CurrentEnrolment;

            if (enrolment is null)
                continue;

            response.Add(new(
                student.Id,
                student.Name.PreferredName, 
                student.Name.LastName,
                enrolment.Grade.AsName()));
        }

        return response;
    }
}