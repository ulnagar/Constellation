using AutoMapper;
using AutoMapper.QueryableExtensions;
using Constellation.Application.Extensions;
using Constellation.Application.Features.Attendance.Queries;
using Constellation.Application.Interfaces.Gateways;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Interfaces.Services;
using Constellation.Core.Models;
using Constellation.Infrastructure.Templates.Views.Documents.Attendance;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;

namespace Constellation.Infrastructure.Features.Attendance.Queries
{
    public class GetStudentAttendanceReportQueryHandler : IRequestHandler<GetStudentAttendanceReportQuery, StoredFile>
    {
        private readonly IAppDbContext _context;
        private readonly ISentralGateway _sentralGateway;
        private readonly IRazorViewToStringRenderer _renderService;
        private readonly IMapper _mapper;
        private readonly IPDFService _pdfService;

        public GetStudentAttendanceReportQueryHandler(
            IAppDbContext context,
            IMapper mapper,
            IPDFService pdfService,
            IRazorViewToStringRenderer renderService)
        {
            _context = context;
            _mapper = mapper;
            _pdfService = pdfService;
            _renderService = renderService;
        }

        public GetStudentAttendanceReportQueryHandler(IAppDbContext context, ISentralGateway sentralGateway, 
            IRazorViewToStringRenderer renderService, IMapper mapper, IPDFService pdfService)
        {
            _context = context;
            _sentralGateway = sentralGateway;
            _renderService = renderService;
            _mapper = mapper;
            _pdfService = pdfService;
        }

        public async Task<StoredFile> Handle(GetStudentAttendanceReportQuery request, CancellationToken cancellationToken)
        {
            if (_sentralGateway is null)
                return null;

            var student = await _context.Students
                .SingleOrDefaultAsync(student => student.StudentId == request.StudentId, cancellationToken);

            var viewModel = new AttendanceReportViewModel
            {
                StudentName = student.DisplayName,
                StartDate = request.StartDate.VerifyStartOfFortnight(),
                ExcludedDates = await _sentralGateway.GetExcludedDatesFromCalendar(request.StartDate.Year.ToString())
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
}
