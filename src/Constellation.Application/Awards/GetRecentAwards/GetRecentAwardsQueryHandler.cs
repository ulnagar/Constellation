namespace Constellation.Application.Awards.GetRecentAwards;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Extensions;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions;
using Constellation.Core.Shared;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetRecentAwardsQueryHandler
    : IQueryHandler<GetRecentAwardsQuery, List<RecentAwardResponse>>
{
    private readonly IStudentAwardRepository _awardRepository;
    private readonly IStudentRepository _studentRepository;

    public GetRecentAwardsQueryHandler(
        IStudentAwardRepository awardRepository,
        IStudentRepository studentRepository)
    {
        _awardRepository = awardRepository;
        _studentRepository = studentRepository;
    }

    public async Task<Result<List<RecentAwardResponse>>> Handle(GetRecentAwardsQuery request, CancellationToken cancellationToken)
    {
        List<RecentAwardResponse> result = new();
        
        var awards = await _awardRepository.GetToRecentCount(request.Count, cancellationToken);

        var students = await _studentRepository.GetCurrentStudentsWithSchool(cancellationToken);

        foreach (var award in awards)
        {
            var student = students.FirstOrDefault(student => student.StudentId == award.StudentId);

            if (student is null)
                continue;

            var entry = new RecentAwardResponse(
                award.Id,
                award.StudentId,
                student.DisplayName,
                student.CurrentGrade.AsName(),
                student.School.Name,
                award.Type,
                award.AwardedOn);

            result.Add(entry);
        }
        
        return result;
    }
}
