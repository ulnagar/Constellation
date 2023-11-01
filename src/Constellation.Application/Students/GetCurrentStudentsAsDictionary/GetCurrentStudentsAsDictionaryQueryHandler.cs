namespace Constellation.Application.Students.GetCurrentStudentsAsDictionary;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Extensions;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Shared;
using Core.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public sealed class GetCurrentStudentsAsDictionaryQueryHandler
    : IQueryHandler<GetCurrentStudentsAsDictionaryQuery, Dictionary<string, string>>
{
    private readonly IStudentRepository _studentRepository;

    public GetCurrentStudentsAsDictionaryQueryHandler(IStudentRepository studentRepository)
    {
        _studentRepository = studentRepository;
    }

    public async Task<Result<Dictionary<string, string>>> Handle(GetCurrentStudentsAsDictionaryQuery request, CancellationToken cancellationToken)
    {
        var students = await _studentRepository.ForSelectionListAsync();

        students = students
            .OrderBy(student => student.CurrentGrade)
            .ThenBy(student => student.LastName)
            .ThenBy(student => student.FirstName)
            .ToList();

        return students.ToDictionary(k => k.StudentId, k => $"{k.DisplayName} ({k.CurrentGrade.AsName()})");
    }
}
