namespace Constellation.Application.Domains.Attendance.Absences.Queries.ExportAttendanceStatisticsReport;

using Abstractions.Messaging;
using Core.Enums;
using Core.Shared;
using Interfaces.Services.Excel;

internal sealed class ExportAttendanceStatisticsReportQueryHandler
    : IQueryHandler<ExportAttendanceStatisticsReportQuery, byte[]>
{
    private readonly IExcelWriter _writer;

    public ExportAttendanceStatisticsReportQueryHandler(
        IExcelWriter writer)
    {
        _writer = writer;
    }

    public async Task<Result<byte[]>> Handle(ExportAttendanceStatisticsReportQuery request, CancellationToken cancellationToken)
    {
        List<(string Category, string Description, decimal Value)> statistics = [];

        foreach (var item in request.Statistics.WholeSchoolAttendancePercentage)
        {
            statistics.Add(("Whole School Attendance", item.Key, item.Value));
        }

        foreach (var item in request.Statistics.CurrentWholeSchoolAttendanceByGrade)
        {
            statistics.Add(("Attendance By Grade", item.Key.Name, item.Value));
        }

        foreach (var grade in Grade.GetOptions.Where(grade => grade.Order > 0).OrderBy(grade => grade.Order))
        {
            statistics.Add(("Partial Absences - Unexplained", grade.Name, request.Statistics.UnexplainedPartialAbsenceCountByGrade[grade]));
            statistics.Add(("Partial Absences - Total", grade.Name, request.Statistics.TotalPartialAbsenceCountByGrade[grade]));
            statistics.Add(("Partial Absences - Percentage", grade.Name, request.Statistics.PartialAbsencePercentageByGrade[grade]));
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
