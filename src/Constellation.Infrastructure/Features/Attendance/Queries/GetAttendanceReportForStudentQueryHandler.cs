namespace Constellation.Infrastructure.Features.Attendance.Queries;

using Constellation.Application.Attendance.GenerateAttendanceReportForStudent;
using Constellation.Application.Extensions;
using Constellation.Application.Interfaces.Gateways;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Interfaces.Services;
using Constellation.Infrastructure.Templates.Views.Documents.Attendance;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public class GetAttendanceReportForStudentQueryHandler : IRequestHandler<GenerateAttendanceReportForStudentQuery, MemoryStream>
{
    private readonly IAppDbContext _context;
    private readonly ISentralGateway _sentralGateway;
    private readonly IRazorViewToStringRenderer _renderService;
    private readonly IPDFService _pdfService;

    public GetAttendanceReportForStudentQueryHandler(
        IAppDbContext context,
        IRazorViewToStringRenderer renderService,
        IPDFService pdfService)
    {
        _context = context;
        _renderService = renderService;
        _pdfService = pdfService;
    }

    public GetAttendanceReportForStudentQueryHandler(IAppDbContext context, ISentralGateway sentralGateway, 
        IRazorViewToStringRenderer renderService, IPDFService pdfService)
    {
        _context = context;
        _sentralGateway = sentralGateway;
        _renderService = renderService;
        _pdfService = pdfService;
    }

    public async Task<MemoryStream> Handle(GenerateAttendanceReportForStudentQuery request, CancellationToken cancellationToken)
    {
        if (_sentralGateway is null)
            return null;

        var student = await _context.Students
            .FirstOrDefaultAsync(student => student.StudentId == request.StudentId, cancellationToken);

        var viewModel = new AttendanceReportViewModel
        {
            StudentName = student.DisplayName,
            StartDate = request.StartDate,
            ExcludedDates = await _sentralGateway.GetExcludedDatesFromCalendar(request.StartDate.Year.ToString())
        };

        var periodAbsences = await _context.Absences
            .Include(absence => absence.Responses)
            .Where(absence => absence.StudentId == request.StudentId && absence.Date >= request.StartDate && absence.Date <= request.EndDate)
            .ToListAsync();

        var convertedAbsences = periodAbsences.Select(AttendanceReportViewModel.AbsenceDto.ConvertFromAbsence).ToList();
        viewModel.Absences = convertedAbsences;

        var ReportableDates = viewModel.StartDate.Range(request.EndDate).ToList();
        foreach (var date in ReportableDates)
        {
            var entry = new AttendanceReportViewModel.DateSessions
            {
                Date = date,
                DayNumber = date.GetDayNumber()
            };

            var sessions = await _context.Sessions
                .Include(s => s.Offering)
                    .ThenInclude(offering => offering.Course)
                .Include(s => s.Period)
                .Where(s =>
                    // Is the Offering (Class) valid for the date selected?
                    s.Offering.StartDate <= date && s.Offering.EndDate >= date &&
                    // Is the Student enrolled in the Offering at the date selected?
                    s.Offering.Enrolments.Any(e => e.StudentId == request.StudentId && e.DateCreated <= date && (!e.DateDeleted.HasValue || e.DateDeleted.Value >= date)) &&
                    // Is the Session valid for the Offering at the date selected?
                    s.DateCreated <= date && (!s.DateDeleted.HasValue || s.DateDeleted.Value >= date) &&
                    // Is the Session for the Cycle Day selected?
                    s.Period.Day == date.GetDayNumber() &&
                    s.Period.Type != "Other")
                .ToListAsync();

            entry.SessionsByOffering = sessions.OrderBy(s => s.Period.StartTime).GroupBy(s => s.OfferingId).Select(AttendanceReportViewModel.SessionByOffering.ConvertFromSessionGroup).ToList();

            viewModel.DateData.Add(entry);
        }

        var headerString = await _renderService.RenderViewToStringAsync("/Views/Documents/Attendance/AttendanceReportHeader.cshtml", viewModel);
        var htmlString = await _renderService.RenderViewToStringAsync("/Views/Documents/Attendance/AttendanceReport.cshtml", viewModel);

        // Save to a temporary file
        return _pdfService.StringToPdfStream(htmlString, headerString);
    }
}