namespace Constellation.Application.Students.GetCurrentStudentsWithSentralId;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Shared;
using Constellation.Core.ValueObjects;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetCurrentStudentsWithSentralIdQueryHandler
    : IQueryHandler<GetCurrentStudentsWithSentralIdQuery, List<StudentWithSentralIdResponse>>
{
    private readonly IStudentRepository _studentRepository;

    public GetCurrentStudentsWithSentralIdQueryHandler(
        IStudentRepository studentRepository)
    {
        _studentRepository = studentRepository;
    }

    public async Task<Result<List<StudentWithSentralIdResponse>>> Handle(GetCurrentStudentsWithSentralIdQuery request, CancellationToken cancellationToken)
    {
        List<StudentWithSentralIdResponse> results = new();

        var students = await _studentRepository.GetCurrentStudentsWithSchool(cancellationToken);

        if (students is null)
            return results;

        foreach (var student in students)
        {
            var studentName = Name.Create(student.FirstName, null, student.LastName);

            if (studentName.IsFailure)
                continue;

            results.Add(new(
                student.StudentId,
                studentName.Value,
                student.CurrentGrade,
                student.SentralStudentId));
        }

        return results;
    }
}
