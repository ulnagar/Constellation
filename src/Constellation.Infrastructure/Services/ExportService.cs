namespace Constellation.Infrastructure.Services;

using Constellation.Application.Attendance.GenerateAttendanceReportForStudent;
using Constellation.Application.Interfaces.Services;
using Constellation.Core.Models.Students;
using Constellation.Infrastructure.Templates.Views.Documents.Attendance;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class ExportService : IExportService
{

    private readonly IRazorViewToStringRenderer _renderService;
    private readonly IPDFService _pdfService;

    public ExportService(
        IRazorViewToStringRenderer renderService,
        IPDFService pdfService)
    {

        _renderService = renderService;
        _pdfService = pdfService;
    }

    public async Task<MemoryStream> CreateAttendanceReport(
        Student student,
        DateOnly startDate,
        List<DateOnly> excludedDates,
        List<AttendanceAbsenceDetail> absences,
        List<AttendanceDateDetail> dates,
        CancellationToken cancellationToken = default)
    {
        var endDate = startDate.AddDays(12);

        var viewModel = new AttendanceReportViewModel
        {
            StudentName = student.Name.DisplayName,
            StartDate = startDate,
            ExcludedDates = excludedDates,
            Absences = absences,
            DateData = dates
        };

        var headerString = await _renderService.RenderViewToStringAsync("/Views/Documents/Attendance/AttendanceReportHeader.cshtml", viewModel);
        var htmlString = await _renderService.RenderViewToStringAsync("/Views/Documents/Attendance/AttendanceReport.cshtml", viewModel);

        return _pdfService.StringToPdfStream(htmlString, headerString, 100);
    }
}
