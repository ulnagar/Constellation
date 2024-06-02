namespace Constellation.Application.Students.GetCurrentStudentsWithoutSentralId;

using Abstractions.Messaging;
using Constellation.Core.Models.Students.Repositories;
using Core.Models.Students;
using Core.Shared;
using Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetCurrentStudentsWithoutSentralIdQueryHandler
: IQueryHandler<GetCurrentStudentsWithoutSentralIdQuery, List<StudentResponse>>
{
    private readonly IStudentRepository _studentRepository;

    public GetCurrentStudentsWithoutSentralIdQueryHandler(
        IStudentRepository studentRepository)
    {
        _studentRepository = studentRepository;
    }

    public async Task<Result<List<StudentResponse>>> Handle(GetCurrentStudentsWithoutSentralIdQuery request, CancellationToken cancellationToken)
    {
        List<StudentResponse> response = new();

        List<Student> students = await _studentRepository.GetCurrentStudentsWithoutSentralId(cancellationToken);

        foreach (Student student in students)
        {
            response.Add(new(
                student.StudentId,
                student.GetName(),
                student.Gender,
                student.CurrentGrade,
                student.PortalUsername,
                student.EmailAddress,
                student.School.Name,
                student.SchoolCode,
                student.IsDeleted));
        }

        return response;
    }
}
