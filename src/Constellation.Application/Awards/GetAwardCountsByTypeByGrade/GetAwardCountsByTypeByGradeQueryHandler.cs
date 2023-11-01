namespace Constellation.Application.Awards.GetAwardCountsByTypeByGrade;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Extensions;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Enums;
using Constellation.Core.Models.Awards;
using Constellation.Core.Shared;
using Core.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetAwardCountsByTypeByGradeQueryHandler
    : IQueryHandler<GetAwardCountsByTypeByGradeQuery, List<AwardCountByTypeByGradeResponse>>
{
    private readonly IStudentRepository _studentRepository;
    private readonly IStudentAwardRepository _awardRepository;

    public GetAwardCountsByTypeByGradeQueryHandler(
        IStudentRepository studentRepository,
        IStudentAwardRepository awardRepository)
    {
        _studentRepository = studentRepository;
        _awardRepository = awardRepository;
    }

    public async Task<Result<List<AwardCountByTypeByGradeResponse>>> Handle(GetAwardCountsByTypeByGradeQuery request, CancellationToken cancellationToken)
    {
        List<AwardCountByTypeByGradeResponse> result = new();

        var students = await _studentRepository.GetCurrentStudentsWithSchool(cancellationToken);

        var awards = await _awardRepository.GetFromYear(request.Year, cancellationToken);

        List<AwardWithGrade> awardsWithGrade = new();

        foreach (var award in awards)
        {
            var student = students.FirstOrDefault(student => student.StudentId == award.StudentId);

            if (student is not null)
            {
                awardsWithGrade.Add(new()
                {
                    Type = award.Type,
                    Grade = student.CurrentGrade,
                    Month = award.AwardedOn.Month
                });
            }

            // If Student is null, they have likely withdrawn and therefore is not included in the "Current Students" retrieved above
        }

        foreach (Grade grade in Enum.GetValues(typeof(Grade)))
        {
            for (int j = 0; j <= 3; j++)
            {
                var awardType = j switch
                {
                    0 => StudentAward.Astra,
                    1 => StudentAward.Stellar,
                    2 => StudentAward.Galaxy,
                    3 => StudentAward.Universal,
                    _ => string.Empty
                };

                var entry = new AwardCountByTypeByGradeResponse(
                    "YTD",
                    grade.AsName(),
                    awardType,
                    awardsWithGrade.Count(award => award.Grade == grade && award.Type == awardType));

                result.Add(entry);
            }
        }

        foreach (Grade grade in Enum.GetValues(typeof(Grade)))
        {
            for (int j = 0; j <= 3; j++)
            {
                var awardType = j switch
                {
                    0 => StudentAward.Astra,
                    1 => StudentAward.Stellar,
                    2 => StudentAward.Galaxy,
                    3 => StudentAward.Universal,
                    _ => string.Empty
                };

                var entry = new AwardCountByTypeByGradeResponse(
                    "This Month",
                    grade.AsName(),
                    awardType,
                    awardsWithGrade.Count(award => award.Grade == grade && award.Type == awardType && award.Month == DateTime.Today.Month));

                result.Add(entry);
            }
        }

        return result;
    }

    private class AwardWithGrade
    {
        public string Type { get; set; }
        public Grade Grade { get; set; }
        public int Month { get; set; }
    }
}
