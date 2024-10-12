namespace Constellation.Application.Students.GetCurrentStudentsWithAwards;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Models.Students.Repositories;
using Constellation.Core.Shared;
using Core.Models.Awards;
using Core.Models.Students;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetCurrentStudentsWithAwardsQueryHandler
    : IQueryHandler<GetCurrentStudentsWithAwardsQuery, List<CurrentStudentWithAwardsResponse>>
{
    private readonly IStudentRepository _studentRepository;
    private readonly IStudentAwardRepository _awardRepository;

    public GetCurrentStudentsWithAwardsQueryHandler(
        IStudentRepository studentRepository,
        IStudentAwardRepository awardRepository)
    {
        _studentRepository = studentRepository;
        _awardRepository = awardRepository;
    }

    public async Task<Result<List<CurrentStudentWithAwardsResponse>>> Handle(GetCurrentStudentsWithAwardsQuery request, CancellationToken cancellationToken)
    {
        List<CurrentStudentWithAwardsResponse> result = new();

        List<Student> students = await _studentRepository.GetCurrentStudents(cancellationToken);

        foreach (Student student in students)
        {
            List<StudentAward> awards = await _awardRepository.GetByStudentId(student.Id, cancellationToken);

            List<CurrentStudentWithAwardsResponse.RegisteredAward> studentAwards = new();

            foreach (StudentAward award in awards)
            {
                studentAwards.Add(new(
                    award.Id,
                    award.Category,
                    award.Type,
                    award.AwardedOn));
            }

            SchoolEnrolment? enrolment = student.CurrentEnrolment;

            if (enrolment is null)
                continue;

            result.Add(new(
                student.Id,
                student.Name.PreferredName,
                student.Name.LastName,
                student.Name.DisplayName,
                enrolment.SchoolCode,
                enrolment.SchoolName,
                enrolment.Grade,
                studentAwards));
        }

        return result;
    }
}
