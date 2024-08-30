namespace Constellation.Application.Students.GetCurrentStudentsFromSchool;

using Abstractions.Messaging;
using Constellation.Core.Models.Students.Repositories;
using Core.Models.Students;
using Core.Shared;
using Models;
using Serilog;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetStudentsFromSchoolQueryHandler 
    : IQueryHandler<GetCurrentStudentsFromSchoolQuery, List<StudentResponse>>
{
    private readonly IStudentRepository _studentRepository;
    private readonly ILogger _logger;

    public GetStudentsFromSchoolQueryHandler(
        IStudentRepository studentRepository,
        ILogger logger)
    {
        _studentRepository = studentRepository;
        _logger = logger;
    }

    public async Task<Result<List<StudentResponse>>> Handle(GetCurrentStudentsFromSchoolQuery request, CancellationToken cancellationToken)
    {
        List<StudentResponse> response = new();

        List<Student> students = await _studentRepository.GetCurrentStudentsFromSchool(request.SchoolCode, cancellationToken);

        foreach (Student student in students)
        {
            SchoolEnrolment? enrolment = student.CurrentEnrolment;

            if (enrolment is null)
                continue;

            StudentResponse viewModel = new(
                student.Id,
                student.StudentReferenceNumber,
                student.Name,
                student.PreferredGender,
                enrolment.Grade,
                student.EmailAddress,
                enrolment.SchoolName,
                enrolment.SchoolCode,
                student.IsDeleted);

            response.Add(viewModel);
        }

        return response;
    }
}