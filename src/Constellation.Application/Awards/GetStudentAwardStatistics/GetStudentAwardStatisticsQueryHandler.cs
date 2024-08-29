namespace Constellation.Application.Awards.GetStudentAwardStatistics;

using Abstractions.Messaging;
using Constellation.Core.Models.Students.Repositories;
using Core.Models.Students;
using Core.Shared;
using Serilog;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetStudentAwardStatisticsQueryHandler
    : IQueryHandler<GetStudentAwardStatisticsQuery, List<StudentAwardStatisticsResponse>>
{
    private readonly IStudentRepository _studentRepository;
    private readonly ILogger _logger;

    public GetStudentAwardStatisticsQueryHandler(
        IStudentRepository studentRepository,
        ILogger logger)
    {
        _studentRepository = studentRepository;
        _logger = logger.ForContext<GetStudentAwardStatisticsQuery>();
    }

    public async Task<Result<List<StudentAwardStatisticsResponse>>> Handle(GetStudentAwardStatisticsQuery request, CancellationToken cancellationToken)
    {
        List<StudentAwardStatisticsResponse> results = new();

        List<Student> students = await _studentRepository.GetCurrentStudents(cancellationToken);

        if (students is null)
            return results;

        foreach (Student student in students)
        {
            SchoolEnrolment? enrolment = student.CurrentEnrolment;

            if (enrolment is null)
                continue;

            results.Add(new(
                student.Id,
                student.Name,
                enrolment.Grade,
                student.AwardTally.Astras,
                student.AwardTally.Stellars,
                student.AwardTally.GalaxyMedals,
                student.AwardTally.UniversalAchievers,
                student.AwardTally.CalculatedStellars,
                student.AwardTally.CalculatedGalaxyMedals,
                student.AwardTally.CalculatedUniversalAchievers));
        }

        return results;
    }
}
