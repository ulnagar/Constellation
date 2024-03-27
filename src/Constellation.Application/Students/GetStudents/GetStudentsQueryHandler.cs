namespace Constellation.Application.Students.GetStudents;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Students.Models;
using Constellation.Core.Models.Students.Errors;
using Constellation.Core.Models.Students.Repositories;
using Constellation.Core.Shared;
using Core.Models.Students;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetStudentsQueryHandler
    : IQueryHandler<GetStudentsQuery, List<StudentResponse>>
{
    private readonly IStudentRepository _studentRepository;

    public GetStudentsQueryHandler(IStudentRepository studentRepository)
    {
        _studentRepository = studentRepository;
    }

    public async Task<Result<List<StudentResponse>>> Handle(GetStudentsQuery request, CancellationToken cancellationToken)
    {
        List<Student> students = await _studentRepository.GetFilteredStudents(new(), new(), new(), cancellationToken);

        if (students.Count == 0)
        {
            return Result.Failure<List<StudentResponse>>(StudentErrors.NotFound(""));
        }

        List<StudentResponse> response = new();

        foreach (Student student in students)
        {
            response.Add(new StudentResponse(
                student.StudentId,
                student.DisplayName,
                student.CurrentGrade,
                student.EmailAddress,
                student.School.Name,
                student.SchoolCode,
                student.IsDeleted));
        }

        return response;
    }
}
