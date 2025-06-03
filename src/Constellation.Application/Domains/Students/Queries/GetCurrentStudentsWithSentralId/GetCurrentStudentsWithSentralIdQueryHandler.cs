namespace Constellation.Application.Domains.Students.Queries.GetCurrentStudentsWithSentralId;

using Abstractions.Messaging;
using Constellation.Core.Enums;
using Constellation.Core.Models.Students.Repositories;
using Constellation.Core.Primitives;
using Core.Models.Students;
using Core.Models.Students.Enums;
using Core.Shared;
using System.Collections.Generic;
using System.Linq;
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

        List<Student> students = await _studentRepository.GetCurrentStudents(cancellationToken);

        if (students is null)
            return results;

        foreach (Student student in students)
        {
            SchoolEnrolment enrolment = student.CurrentEnrolment;

            if (enrolment is null) 
                continue;

            SystemLink link = student.SystemLinks.FirstOrDefault(entry => entry.System == SystemType.Sentral);

            if (link is null)
                continue;

            results.Add(new(
                student.Id,
                student.Name,
                enrolment.Grade,
                link.Value));
        }

        return results;
    }
}
