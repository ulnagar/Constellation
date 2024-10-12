namespace Constellation.Application.Students.GetCurrentStudentsAsDictionary;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.Students.Repositories;
using Constellation.Core.Shared;
using Core.Extensions;
using Core.Models.Students;
using Core.Models.Students.Identifiers;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public sealed class GetCurrentStudentsAsDictionaryQueryHandler
    : IQueryHandler<GetCurrentStudentsAsDictionaryQuery, Dictionary<StudentId, string>>
{
    private readonly IStudentRepository _studentRepository;

    public GetCurrentStudentsAsDictionaryQueryHandler(IStudentRepository studentRepository)
    {
        _studentRepository = studentRepository;
    }

    public async Task<Result<Dictionary<StudentId, string>>> Handle(GetCurrentStudentsAsDictionaryQuery request, CancellationToken cancellationToken)
    {
        List<Student> students = await _studentRepository.GetCurrentStudents(cancellationToken);

        students = students
            .OrderBy(student => student.CurrentEnrolment?.Grade)
            .ThenBy(student => student.Name.SortOrder)  
            .ToList();

        return students.ToDictionary(k => k.Id, k => $"{k.Name.DisplayName} ({k.CurrentEnrolment?.Grade.AsName()})");
    }
}
