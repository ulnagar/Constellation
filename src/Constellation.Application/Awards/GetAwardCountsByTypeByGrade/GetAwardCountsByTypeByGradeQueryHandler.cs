namespace Constellation.Application.Awards.GetAwardCountsByTypeByGrade;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Extensions;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions;
using Constellation.Core.Enums;
using Constellation.Core.Shared;
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

        foreach (Grade grade in Enum.GetValues(typeof(Grade)))
        {
            for (int j = 0; j <= 3; j++)
            {
                var awardType = j switch
                {
                    0 => "Astra Award",
                    1 => "Stellar Award",
                    2 => "Galaxy Medal",
                    3 => "Aurora Universal Achiever",
                    _ => ""
                };

                var entry = new AwardCountByTypeByGradeResponse(
                    "YTD",
                    grade.AsName(),
                    awardType,
                    awards
                        .Select(award => new { 
                            Type = award.Type, 
                            Grade = students
                                .First(student => student.StudentId == award.StudentId)
                                .CurrentGrade })
                        .Count(award => 
                            award.Type == awardType && 
                            award.Grade == grade));

                result.Add(entry);
            }
        }

        foreach (Grade grade in Enum.GetValues(typeof(Grade)))
        {
            for (int j = 0; j <= 3; j++)
            {
                var awardType = j switch
                {
                    0 => "Astra Award",
                    1 => "Stellar Award",
                    2 => "Galaxy Medal",
                    3 => "Aurora Universal Achiever",
                    _ => ""
                };

                var entry = new AwardCountByTypeByGradeResponse(
                    "This Month",
                    grade.AsName(),
                    awardType,
                    awards
                        .Select(award => new { 
                            Type = award.Type, 
                            Grade = students
                                .First(student => student.StudentId == award.StudentId)
                                .CurrentGrade, 
                            Month = award.AwardedOn.Month })
                        .Count(award => 
                            award.Type == awardType && 
                            award.Grade == grade &&
                            award.Month == DateTime.Today.Month));

                result.Add(entry);
            }
        }

        return result;
    }
}
