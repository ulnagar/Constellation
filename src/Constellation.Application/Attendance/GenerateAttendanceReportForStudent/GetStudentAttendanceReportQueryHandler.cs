namespace Constellation.Application.Attendance.GenerateAttendanceReportForStudent;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Extensions;
using Constellation.Application.Features.Attendance.Queries;
using Constellation.Application.Interfaces.Gateways;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Interfaces.Services;
using Constellation.Core.Models;
using Constellation.Core.Shared;
using System;
using System.Collections.Generic;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;

public class GenerateAttendanceReportForStudentQueryHandler 
    : IQueryHandler<GenerateAttendanceReportForStudentQuery, StoredFile>
{
    private readonly IStudentRepository _studentRepository;
    private readonly ISentralGateway _sentralGateway;
    private readonly IRazorViewToStringRenderer _renderService;
    private readonly IPDFService _pdfService;

    public GenerateAttendanceReportForStudentQueryHandler(
        IStudentRepository studentRepository,

        ISentralGateway sentralGateway,
        IRazorViewToStringRenderer renderService,
        IPDFService pdfService)
    {
        _studentRepository = studentRepository;
        _sentralGateway = sentralGateway;
        _renderService = renderService;
        _pdfService = pdfService;
    }

    public async Task<Result<StoredFile>> Handle(GenerateAttendanceReportForStudentQuery request, CancellationToken cancellationToken)
    {
        List<DateOnly> excludedDates = await _sentralGateway.GetExcludedDatesFromCalendar(request.StartDate.Year.ToString());

        Student student = await _studentRepository.GetById(request.StudentId, cancellationToken);

        var viewModel = new AttendanceReportViewModel
        {
            StudentName = student.DisplayName,
            StartDate = request.StartDate.VerifyStartOfFortnight(),
            ExcludedDates = excludedDates
        };

        var endDate = viewModel.StartDate.AddDays(12);
        var absences = await _context.Absences
            .Where(absence => absence.StudentId == student.StudentId && absence.Date <= endDate && absence.Date >= viewModel.StartDate)
            .ToListAsync(cancellationToken);

        viewModel.Absences = absences.Select(AttendanceReportViewModel.AbsenceDto.ConvertFromAbsence).ToList();

        var ReportableDates = viewModel.StartDate.Range(endDate).ToList();
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

        var fileName = $"{student.LastName}, {student.FirstName} - {request.StartDate:yyyy-MM-dd} - Attendance Report.pdf";

        var headerString = await _renderService.RenderViewToStringAsync("/Views/Documents/Attendance/AttendanceReportHeader.cshtml", viewModel);
        var htmlString = await _renderService.RenderViewToStringAsync("/Views/Documents/Attendance/AttendanceReport.cshtml", viewModel);

        var pdfStream = _pdfService.StringToPdfStream(htmlString, headerString);

        var result = new StoredFile
        {
            Name = fileName,
            FileData = pdfStream.ToArray(),
            FileType = MediaTypeNames.Application.Pdf
        };

        return result;
    }
}
