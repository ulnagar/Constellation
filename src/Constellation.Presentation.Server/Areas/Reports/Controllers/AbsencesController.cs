using Constellation.Application.Extensions;
using Constellation.Application.Families.GetResidentialFamilyEmailAddresses;
using Constellation.Application.Families.GetResidentialFamilyMobileNumbers;
using Constellation.Application.Features.Jobs.AbsenceMonitor.Queries;
using Constellation.Application.Interfaces.Gateways;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Interfaces.Services;
using Constellation.Application.Models.Auth;
using Constellation.Core.Models.Absences;
using Constellation.Infrastructure.Templates.Views.Documents.Attendance;
using Constellation.Presentation.Server.Areas.Reports.Models;
using Constellation.Presentation.Server.BaseModels;
using Constellation.Presentation.Server.Helpers.Attributes;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Threading.Tasks;

namespace Constellation.Presentation.Server.Areas.Reports.Controllers
{
    [Area("Reports")]
    [Roles(AuthRoles.Admin, AuthRoles.AbsencesEditor, AuthRoles.Editor, AuthRoles.StaffMember)]
    public class AbsencesController : BaseController
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailService _emailService;
        private readonly ISMSService _smsService;
        private readonly ISentralGateway _sentralGateway;
        private readonly IRazorViewToStringRenderer _renderService;
        private readonly IExportService _exportService;
        private readonly IMediator _mediator;

        public AbsencesController(IUnitOfWork unitOfWork, IEmailService emailService, 
            ISMSService smsService, ISentralGateway sentralGateway,
            IRazorViewToStringRenderer renderService, IExportService exportService,
            IMediator mediator)
            : base(unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _emailService = emailService;
            _smsService = smsService;
            _sentralGateway = sentralGateway;
            _renderService = renderService;
            _exportService = exportService;
            _mediator = mediator;
        }

        public IActionResult Index()
        {
            return RedirectToAction("FilterSelection");
        }

        public async Task<IActionResult> FilterSelection()
        {
            var schools = await _unitOfWork.Schools.ForSelectionAsync();
            var students = await _unitOfWork.Students.ForSelectionListAsync();

            var viewModel = await CreateViewModel<Absence_FilterSelectionViewModel>();
            viewModel.SchoolList = new SelectList(schools.OrderBy(school => school.Name), "Code", "Name");
            viewModel.StudentList = new SelectList(students.OrderBy(student => student.CurrentGrade).ThenBy(student => student.LastName), "StudentId", "DisplayName");

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> ReportAction(Absence_FilterSelectionViewModel viewModel)
        {
            var absences = await _unitOfWork.Absences.ForReportAsync(viewModel.Filter);

            absences = absences.OrderBy(a => a.Date.Date)
                .ThenBy(a => a.PeriodTimeframe)
                .ThenBy(a => a.Student.School.Name)
                .ThenBy(a => a.Student.CurrentGrade)
                .Where(a => a.Date.Year == DateTime.Now.Year)
                .ToList();

            return await AbsenceReport(absences, viewModel);
        }

        public async Task<IActionResult> AbsenceReport(ICollection<Absence> absences, Absence_FilterSelectionViewModel viewModel)
        {
            var returnViewModel = await CreateViewModel<Absence_ReportViewModel>();
            returnViewModel.Filter = viewModel.Filter;
            returnViewModel.Absences = absences.Select(Absence_ReportViewModel.AbsenceDto.ConvertFromAbsence).ToList();

            return View("AbsenceReport", returnViewModel);
        }

        public async Task<IActionResult> BuildAbsencesExport(Absence_ReportViewModel viewModel)
        {
            var data = await _exportService.CreateAbsenceExport(viewModel.Filter);

            TempData["data"] = JsonConvert.SerializeObject(data);

            return RedirectToAction("ExportAbsences", "ExcelExport", new { area = "Utility", title = "Absences" });
        }

        [Roles(AuthRoles.Admin, AuthRoles.AbsencesEditor)]
        public async Task<IActionResult> SendNotification(string id, string method, CancellationToken cancellationToken = default)
        {
            var absence = await _unitOfWork.Absences.ForSendingNotificationAsync(id);
            var absences = new List<Absence> { absence };

            if (string.IsNullOrWhiteSpace(absence.Student.SentralStudentId))
            {
                await _emailService.SendAdminAbsenceSentralAlert(absence.Student.DisplayName);
                return RedirectToAction("FilterSelection");
            }

            if (absence.Type == Absence.Whole)
            {
                switch (method)
                {
                    case AbsenceNotification.SMS:
                        {
                            var phoneNumbers = await _mediator.Send(new GetResidentialFamilyMobileNumbersQuery(absence.StudentId), cancellationToken);
                            if (phoneNumbers.IsFailure || !phoneNumbers.Value.Any())
                                break;

                            var sentMessage = await _smsService.SendAbsenceNotificationAsync(absences, phoneNumbers.Value);

                            if (sentMessage.Messages.Count > 0)
                            {
                                absence.Notifications.Add(new AbsenceNotification
                                {
                                    Type = AbsenceNotification.SMS,
                                    SentAt = DateTime.Now,
                                    Message = sentMessage.Messages.First().MessageBody,
                                    Recipients = phoneNumbers.Value.Select(entry => entry.ToString(Core.ValueObjects.PhoneNumber.Format.None)).ToList().Collapse('|'),
                                    OutgoingId = sentMessage.Messages.First().OutgoingId
                                });
                            }

                            break;
                        }

                    case AbsenceNotification.Email:
                        {
                            var emailAddresses = await _mediator.Send(new GetResidentialFamilyEmailAddressesQuery(absence.StudentId), cancellationToken);
                            if (emailAddresses.IsFailure || !emailAddresses.Value.Any())
                                break;

                            var sentMessage = await _emailService.SendParentWholeAbsenceAlert(absences, emailAddresses.Value);

                            absence.Notifications.Add(new AbsenceNotification
                            {
                                Type = AbsenceNotification.Email,
                                SentAt = DateTime.Now,
                                Message = sentMessage.message,
                                Recipients = emailAddresses.Value.Select(entry => entry.Email).ToList().Collapse('|'),
                                OutgoingId = sentMessage.id
                            });

                            break;
                        }
                }
            }

            if (absence.Type == Absence.Partial)
            {
                var recipients = new List<string> { absence.Student.EmailAddress };
                var sentMessage = await _emailService.SendStudentPartialAbsenceExplanationRequest(absences, recipients.ToList());

                absence.Notifications.Add(new AbsenceNotification
                {
                    Type = AbsenceNotification.Email,
                    SentAt = DateTime.Now,
                    Message = sentMessage.message,
                    Recipients = recipients.Collapse('|'),
                    OutgoingId = sentMessage.id
                });
            }

            await _unitOfWork.CompleteAsync(cancellationToken);

            return RedirectToAction("FilterSelection");
        }

        [AllowAnonymous]
        public async Task<IActionResult> FortnightReport(string studentId, DateTime startDate)
        {
            var student = await _unitOfWork.Students.ForEditAsync(studentId);

            var viewModel = new AttendanceReportViewModel
            {
                StudentName = student.DisplayName,
                StartDate = startDate.VerifyStartOfFortnight(),
                ExcludedDates = await _sentralGateway.GetExcludedDatesFromCalendar(startDate.Year.ToString())
            };

            var endDate = viewModel.StartDate.AddDays(12);
            var periodAbsences = await _unitOfWork.Absences
                .ForStudentWithinTimePeriod(studentId, viewModel.StartDate, endDate);
            var convertedAbsences = periodAbsences.Select(AttendanceReportViewModel.AbsenceDto.ConvertFromAbsence).ToList();
            viewModel.Absences = convertedAbsences;

            //viewModel.Absences = (await _unitOfWork.Absences
            //    .AllWithFilter(a => a.StudentId == student.StudentId && a.Date <= endDate && a.Date >= viewModel.StartDate))
            //    .Select(AttendanceReportViewModel.AbsenceDto.ConvertFromAbsence).ToList();

            var ReportableDates = viewModel.StartDate.Range(endDate).ToList();
            foreach (var date in ReportableDates)
            {
                var entry = new AttendanceReportViewModel.DateSessions
                {
                    Date = date,
                    DayNumber = date.GetDayNumber()
                };

                var sessions = await _unitOfWork.OfferingSessions.ForStudentAndDayAtTime(studentId, date.GetDayNumber(), date);
                entry.SessionsByOffering = sessions.OrderBy(s => s.Period.StartTime).GroupBy(s => s.OfferingId).Select(AttendanceReportViewModel.SessionByOffering.ConvertFromSessionGroup).ToList();

                viewModel.DateData.Add(entry);
            }

            var headerString = await _renderService.RenderViewToStringAsync("/Views/Documents/Attendance/AttendanceReportHeader.cshtml", viewModel);
            var htmlString = await _renderService.RenderViewToStringAsync("/Views/Documents/Attendance/AttendanceReport.cshtml", viewModel);

            return Json(new { header = headerString, body = htmlString });
        }

        [AllowAnonymous]
        public async Task<IActionResult> AttendanceReportDownload(string studentId, DateTime startDate)
        {
            var student = await _unitOfWork.Students.ForEditAsync(studentId);

            var viewModel = new AttendanceReportViewModel
            {
                StudentName = student.DisplayName,
                StartDate = startDate.VerifyStartOfFortnight(),
                ExcludedDates = await _sentralGateway.GetExcludedDatesFromCalendar(startDate.Year.ToString())
            };

            var endDate = viewModel.StartDate.AddDays(12);
            viewModel.Absences = (await _unitOfWork.Absences
                .AllWithFilter(a => a.StudentId == student.StudentId && a.Date <= endDate && a.Date >= viewModel.StartDate))
                .Select(AttendanceReportViewModel.AbsenceDto.ConvertFromAbsence).ToList();

            var ReportableDates = viewModel.StartDate.Range(endDate).ToList();
            foreach (var date in ReportableDates)
            {
                var entry = new AttendanceReportViewModel.DateSessions
                {
                    Date = date,
                    DayNumber = date.GetDayNumber()
                };

                var sessions = await _unitOfWork.OfferingSessions.ForStudentAndDayAtTime(studentId, date.GetDayNumber(), date);
                entry.SessionsByOffering = sessions.OrderBy(s => s.Period.StartTime).GroupBy(s => s.OfferingId).Select(AttendanceReportViewModel.SessionByOffering.ConvertFromSessionGroup).ToList();

                viewModel.DateData.Add(entry);
            }

            var fileName = $"{student.LastName}, {student.FirstName} - {startDate:yyyy-MM-dd} - Attendance Report.pdf";

            TempData["data"] = JsonConvert.SerializeObject(viewModel);

            return RedirectToAction("ExportAttendanceReport", "DocumentExport", new { area = "Utility", title = fileName });
        }
    }
}