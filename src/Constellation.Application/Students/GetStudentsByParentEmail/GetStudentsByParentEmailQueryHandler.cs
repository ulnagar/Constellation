namespace Constellation.Application.Students.GetStudentsByParentEmail;

using Abstractions.Messaging;
using Constellation.Core.Models.Students.Repositories;
using Core.Abstractions.Repositories;
using Core.Extensions;
using Core.Models.Students;
using Core.Shared;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public sealed class GetStudentsByParentEmailQueryHandler 
    : IQueryHandler<GetStudentsByParentEmailQuery, List<StudentResponse>>
{
    private readonly IFamilyRepository _familyRepository;
    private readonly IStudentRepository _studentRepository;

    public GetStudentsByParentEmailQueryHandler(
        IFamilyRepository familyRepository,
        IStudentRepository studentRepository)
    {
        _familyRepository = familyRepository;
        _studentRepository = studentRepository;
    }

    public async Task<Result<List<StudentResponse>>> Handle(GetStudentsByParentEmailQuery request, CancellationToken cancellationToken)
    {
        List<StudentResponse> response = new();

        Dictionary<string, bool> studentIds = await _familyRepository.GetStudentIdsFromFamilyWithEmail(request.ParentEmail, cancellationToken);

        List<Student> students = await _studentRepository.GetListFromIds(studentIds.Keys.ToList(), cancellationToken);

        foreach (Student student in students)
        {
            response.Add(new(
                student.StudentId,
                student.FirstName,
                student.LastName,
                student.CurrentGrade.AsName()));
        }

        return response;
    }
}