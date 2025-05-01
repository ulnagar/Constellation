namespace Constellation.Application.Domains.MeritAwards.Awards.Queries.GetAwardCountsByTypeByGrade;

using Abstractions.Messaging;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Enums;
using Constellation.Core.Models.Awards;
using Constellation.Core.Models.Students.Repositories;
using Core.Extensions;
using Core.Models.Students;
using Core.Shared;
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

        List<Student> students = await _studentRepository.GetCurrentStudents(cancellationToken);

        List<StudentAward> awards = await _awardRepository.GetFromYear(request.Year, cancellationToken);

        List<AwardWithGrade> awardsWithGrade = new();

        foreach (StudentAward award in awards)
        {
            Student student = students.FirstOrDefault(student => student.Id == award.StudentId);

            // If Student is null, they have likely withdrawn and therefore is not included in the "Current Students" retrieved above
            if (student is null)
                continue;

            SchoolEnrolment enrolment = student.CurrentEnrolment;

            if (enrolment is null)
                continue;

            awardsWithGrade.Add(new()
            {
                Type = award.Type,
                Grade = enrolment.Grade,
                Month = award.AwardedOn.Month
            });
        }

        foreach (Grade grade in Enum.GetValues(typeof(Grade)))
        {
            for (int j = 0; j <= 3; j++)
            {
                string awardType = j switch
                {
                    0 => StudentAward.Astra,
                    1 => StudentAward.Stellar,
                    2 => StudentAward.Galaxy,
                    3 => StudentAward.Universal,
                    _ => string.Empty
                };

                AwardCountByTypeByGradeResponse entry = new AwardCountByTypeByGradeResponse(
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
                string awardType = j switch
                {
                    0 => StudentAward.Astra,
                    1 => StudentAward.Stellar,
                    2 => StudentAward.Galaxy,
                    3 => StudentAward.Universal,
                    _ => string.Empty
                };

                AwardCountByTypeByGradeResponse entry = new AwardCountByTypeByGradeResponse(
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
        public string Type { get; init; }
        public Grade Grade { get; init; }
        public int Month { get; init; }
    }
}
