namespace Constellation.Application.Domains.Attendance.Absences.Queries.ExportAttendanceStatisticsReport;

using Abstractions.Messaging;
using Core.Abstractions.Repositories;
using Core.Enums;
using Core.Models.Absences;
using Core.Models.Absences.Enums;
using Core.Models.Attendance;
using Core.Models.Attendance.Repositories;
using Core.Models.Students;
using Core.Models.Students.Identifiers;
using Core.Models.Students.Repositories;
using Core.Shared;
using DTOs;
using Interfaces.Services.Excel;

internal sealed class ExportAttendanceStatisticsReportQueryHandler
    : IQueryHandler<ExportAttendanceStatisticsReportQuery, byte[]>
{
    private readonly IAttendanceRepository _attendanceRepository;
    private readonly IAbsenceRepository _absenceRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly IExcelWriter _writer;

    public ExportAttendanceStatisticsReportQueryHandler(
        IAttendanceRepository attendanceRepository,
        IAbsenceRepository absenceRepository,
        IStudentRepository studentRepository,
        IExcelWriter writer)
    {
        _attendanceRepository = attendanceRepository;
        _absenceRepository = absenceRepository;
        _studentRepository = studentRepository;
        _writer = writer;
    }

    public async Task<Result<byte[]>> Handle(ExportAttendanceStatisticsReportQuery request, CancellationToken cancellationToken)
    {
        List<AttendanceValue> recentWholeSchool = await _attendanceRepository.GetAllRecent(cancellationToken);

        List<IGrouping<string, AttendanceValue>> byPeriod = recentWholeSchool.GroupBy(value => value.PeriodLabel).ToList();

        List<(string Category, string Description, decimal Value)> statistics = [];

        foreach (IGrouping<string, AttendanceValue> period in byPeriod)
        {
            decimal average = period.Average(item => item.PerMinuteYearToDatePercentage);

            statistics.Add((Category: "Whole School Attendance", Description: period.Key, Value: average));
        }

        DateOnly latestDate = recentWholeSchool.Select(entry => entry.EndDate).Max();
        List<IGrouping<Grade, AttendanceValue>> byGrade = recentWholeSchool.Where(entry => entry.EndDate == latestDate).GroupBy(value => value.Grade).ToList();

        foreach (IGrouping<Grade, AttendanceValue> grade in byGrade)
        {
            decimal average = grade.Average(item => item.PerMinuteYearToDatePercentage);

            statistics.Add((Category: "Attendance By Grade", Description: grade.Key.Name, Value: average));
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

            statistics.Add((Category: "Partial Absences - Unexplained", Description: grade.Name, Value: unexplainedPartialAbsences));
            statistics.Add((Category: "Partial Absences - Percentage", Description: grade.Name, Value: partialPercentage));
            statistics.Add((Category: "Partial Absences - Total", Description: grade.Name, Value: partialAbsences.Count));

            List<Absence> wholeAbsences = absencesByGrade.Where(entry => entry.Type == AbsenceType.Whole).ToList();
            int unexplainedWholeAbsences = wholeAbsences.Count(absence => absence.Explained == false);
            decimal wholePercentage = wholeAbsences.Count == 0
                ? 0
                : (decimal)unexplainedWholeAbsences / wholeAbsences.Count * 100;

            statistics.Add((Category: "Whole Absences - Unexplained", Description: grade.Name, Value: unexplainedWholeAbsences));
            statistics.Add((Category: "Whole Absences - Percentage", Description: grade.Name, Value: wholePercentage));
            statistics.Add((Category: "Whole Absences - Total", Description: grade.Name, Value: wholeAbsences.Count));
        }

        IExcelWorkbook workbook = _writer.CreateWorkbook();
        IExcelWorksheet sheet = _writer.AddWorksheet(workbook, "Sheet 1");

        _writer.WriteRange(sheet, 2, statistics,
            new ("Category", a => a.Category),
            new ("Description", a => a.Description),
            new ("Value", a => a.Value));

        _writer.ApplyHeaderStyle(sheet, 1);
        _writer.AddAutoFilter(sheet);
        _writer.AutoFitColumns(sheet);

        byte[] file = _writer.GetAsByteArray(workbook);

        return file;
    }
}
