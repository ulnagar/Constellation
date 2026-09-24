namespace Constellation.Application.Domains.Attendance.Absences.Queries.GetAttendanceStatistics;

using Abstractions.Messaging;
using Constellation.Application.Domains.Attendance.Absences.Models;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Enums;
using Constellation.Core.Models.Absences;
using Constellation.Core.Models.Absences.Enums;
using Constellation.Core.Models.Attendance;
using Constellation.Core.Models.Attendance.Repositories;
using Constellation.Core.Models.Students.Identifiers;
using Constellation.Core.Models.Students.Repositories;
using Core.Models.Students;
using Core.Shared;
using Models;

internal sealed class GetAttendanceStatisticsQueryHandler
: IQueryHandler<GetAttendanceStatisticsQuery, AttendanceStatisticsResponse>
{
    private readonly IAttendanceRepository _attendanceRepository;
    private readonly IAbsenceRepository _absenceRepository;
    private readonly IStudentRepository _studentRepository;

    public GetAttendanceStatisticsQueryHandler(
        IAttendanceRepository attendanceRepository,
        IAbsenceRepository absenceRepository,
        IStudentRepository studentRepository)
    {
        _attendanceRepository = attendanceRepository;
        _absenceRepository = absenceRepository;
        _studentRepository = studentRepository;
    }

    public async Task<Result<AttendanceStatisticsResponse>> Handle(GetAttendanceStatisticsQuery request, CancellationToken cancellationToken)
    {
        AttendanceStatisticsResponse response = new();

        List<AttendanceValue> recentWholeSchool = await _attendanceRepository.GetAllRecent(cancellationToken);

        List<IGrouping<string, AttendanceValue>> byPeriod = recentWholeSchool.GroupBy(value => value.PeriodLabel).ToList();
        
        foreach (IGrouping<string, AttendanceValue> period in byPeriod)
        {
            decimal average = period.Average(item => item.PerMinuteYearToDatePercentage);

            response.WholeSchoolAttendancePercentage.TryAdd(period.Key, average);
        }

        DateOnly latestDate = recentWholeSchool.Select(entry => entry.EndDate).Max();
        List<IGrouping<Grade, AttendanceValue>> byGrade = recentWholeSchool.Where(entry => entry.EndDate == latestDate).GroupBy(value => value.Grade).ToList();

        foreach (IGrouping<Grade, AttendanceValue> grade in byGrade)
        {
            decimal average = grade.Average(item => item.PerMinuteYearToDatePercentage);

            response.CurrentWholeSchoolAttendanceByGrade.TryAdd(grade.Key, average);
        }

        List<Absence> absences = await _absenceRepository.GetAllFromCurrentYear(cancellationToken);
        List<Student> students = await _studentRepository.GetCurrentStudents(cancellationToken);

        foreach (Grade grade in Grade.GetOptions.Where(grade => grade.Order > 0))
        {
            List<StudentId> gradeStudentIds = students
                .Where(student => (student.CurrentEnrolment?.Grade ?? Grade.Empty) == grade)
                .Select(student => student.Id)
                .ToList();

            List<Absence> absencesByGrade = absences
                .Where(absence => gradeStudentIds.Contains(absence.StudentId))
                .ToList();

            List<Absence> partialAbsences = absencesByGrade.Where(entry => entry.Type == AbsenceType.Partial).ToList();
            int unexplainedPartialAbsences = partialAbsences.Count(absence => absence.Explained == false);
            decimal partialPercentage = partialAbsences.Count == 0
                ? 0
                : (decimal)unexplainedPartialAbsences / partialAbsences.Count * 100;

            response.UnexplainedPartialAbsenceCountByGrade.TryAdd(grade, unexplainedPartialAbsences);
            response.PartialAbsencePercentageByGrade.TryAdd(grade, partialPercentage);
            response.TotalPartialAbsenceCountByGrade.TryAdd(grade, partialAbsences.Count);

            List<Absence> wholeAbsences = absencesByGrade.Where(entry => entry.Type == AbsenceType.Whole).ToList();
            int unexplainedWholeAbsences = wholeAbsences.Count(absence => absence.Explained == false);
            decimal wholePercentage = wholeAbsences.Count == 0
                ? 0
                : (decimal)unexplainedWholeAbsences / wholeAbsences.Count * 100;

            response.UnexplainedWholeAbsenceCountByGrade.TryAdd(grade, unexplainedWholeAbsences);
            response.WholeAbsencePercentageByGrade.TryAdd(grade, wholePercentage);
            response.TotalWholeAbsenceCountByGrade.TryAdd(grade, wholeAbsences.Count);
        }

        return response;
    }
}
