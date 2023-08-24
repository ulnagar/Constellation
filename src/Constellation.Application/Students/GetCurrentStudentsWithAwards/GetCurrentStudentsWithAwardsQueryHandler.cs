namespace Constellation.Application.Students.GetCurrentStudentsWithAwards;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Shared;
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

        var students = await _studentRepository.GetCurrentStudentsWithSchool(cancellationToken);

        foreach (var student in students)
        {
            var awards = await _awardRepository.GetByStudentId(student.StudentId, cancellationToken);

            List<CurrentStudentWithAwardsResponse.RegisteredAward> studentAwards = new();

            foreach (var award in awards)
            {
                studentAwards.Add(new(
                    award.Id,
                    award.Category,
                    award.Type,
                    award.AwardedOn));
            }

            result.Add(new(
                student.StudentId,
                student.FirstName,
                student.LastName,
                student.DisplayName,
                student.SchoolCode,
                student.School.Name,
                student.CurrentGrade,
                studentAwards));
        }

        return result;
    }
}
