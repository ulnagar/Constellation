namespace Constellation.Presentation.Staff.Areas.Staff.Pages.StudentAdmin.Attendance;

using Application.Domains.Attendance.Absences.Commands.SetAbsenceConfigurationForStudent;
using Constellation.Application.Common.PresentationModels;
using Constellation.Application.Helpers;
using Constellation.Application.Models.Auth;
using Constellation.Application.Schools.GetSchoolsForSelectionList;
using Constellation.Application.Schools.Models;
using Constellation.Application.Students.GetCurrentStudentsAsDictionary;
using Constellation.Core.Abstractions.Clock;
using Constellation.Core.Abstractions.Services;
using Constellation.Core.Enums;
using Constellation.Core.Models.Absences;
using Constellation.Core.Models.Students.Identifiers;
using Constellation.Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using Presentation.Shared.Helpers.Logging;
using Serilog;
using System.Threading;

[Authorize(Policy = AuthPolicies.CanManageAbsences)]
public class SettingsModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeProvider _dateTime;
    private readonly ILogger _logger;

    public SettingsModel(
        ISender mediator,
        LinkGenerator linkGenerator,
        ICurrentUserService currentUserService,
        IDateTimeProvider dateTime,
        ILogger logger)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
        _currentUserService = currentUserService;
        _dateTime = dateTime;
        _logger = logger
            .ForContext<SettingsModel>()
            .ForContext(LogDefaults.Application, LogDefaults.StaffPortal);
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.StudentAdmin_Attendance_Configuration;
    [ViewData] public string PageTitle => "Student Absence Settings";

    public SelectList Schools { get; set; }
    public SelectList Students { get; set; }
    public SelectList Grades { get; set; }

    [BindProperty] 
    public StudentId StudentId { get; set; } = StudentId.Empty;
    [BindProperty]
    public string? SchoolCode { get; set; }
    [BindProperty]
    public int? Grade { get; set; }

    [BindProperty]
    public string Type { get; set; }
    [BindProperty]
    public DateOnly StartDate { get; set; }
    [BindProperty]
    public DateOnly? EndDate { get; set; }

    public async Task OnGet(CancellationToken cancellationToken = default) => await PreparePage(cancellationToken);

    public async Task<IActionResult> OnPost(CancellationToken cancellationToken = default)
    {
        if (StudentId == StudentId.Empty && string.IsNullOrWhiteSpace(SchoolCode) && !Grade.HasValue)
        {
            Error error = new("Validation.Page.EmptyValues", "You must select a value for Student or Grade or School to continue");

            _logger
                .ForContext(nameof(Error), error, true)
                .Warning("Failed to create new Student Absence Setting by user {User}", _currentUserService.UserName);

            ModalContent = new ErrorDisplay(error);

            await PreparePage(cancellationToken);

            return Page();
        }

        if (Type == "Both")
        {
            SetAbsenceConfigurationForStudentCommand partialCommand = new(
                StudentId,
                SchoolCode,
                Grade,
                AbsenceType.Partial,
                StartDate,
                EndDate);

            _logger
                .ForContext(nameof(SetAbsenceConfigurationForStudentCommand), partialCommand, true)
                .Information("Requested to create new Student Absence Setting by user {User}", _currentUserService.UserName);

            Result partialRequest = await _mediator.Send(partialCommand, cancellationToken);

            if (partialRequest.IsFailure)
            {
                _logger
                    .ForContext(nameof(Error), partialRequest.Error, true)
                    .Warning("Failed to create new Student Absence Setting by user {User}", _currentUserService.UserName);

                ModalContent = new ErrorDisplay(
                    partialRequest.Error,
                    _linkGenerator.GetPathByPage(page: "/StudentAdmin/Attendance/Configuration", values: new { area = "Staff" }));

                return Page();
            }

            SetAbsenceConfigurationForStudentCommand wholeCommand = new(
                StudentId,
                SchoolCode,
                Grade,
                AbsenceType.Whole,
                StartDate,
                EndDate);

            _logger
                .ForContext(nameof(SetAbsenceConfigurationForStudentCommand), wholeCommand, true)
                .Information("Requested to create new Student Absence Setting by user {User}", _currentUserService.UserName);

            Result wholeRequest = await _mediator.Send(wholeCommand, cancellationToken);

            if (wholeRequest.IsFailure)
            {
                _logger
                    .ForContext(nameof(Error), wholeRequest.Error, true)
                    .Warning("Failed to create new Student Absence Setting by user {User}", _currentUserService.UserName);

                ModalContent = new ErrorDisplay(
                    wholeRequest.Error,
                    _linkGenerator.GetPathByPage(page: "/StudentAdmin/Attendance/Configuration", values: new { area = "Staff" }));

                return Page();
            }
        }
        else
        {
            SetAbsenceConfigurationForStudentCommand command = new(
                StudentId,
                SchoolCode,
                Grade,
                AbsenceType.FromValue(Type),
                StartDate,
                EndDate);

            _logger
                .ForContext(nameof(SetAbsenceConfigurationForStudentCommand), command, true)
                .Information("Requested to create new Student Absence Setting by user {User}", _currentUserService.UserName);

            Result request = await _mediator.Send(command, cancellationToken);

            if (request.IsFailure)
            {
                _logger
                    .ForContext(nameof(Error), request.Error, true)
                    .Warning("Failed to create new Student Absence Setting by user {User}", _currentUserService.UserName);

                ModalContent = new ErrorDisplay(
                    request.Error,
                    _linkGenerator.GetPathByPage(page: "/StudentAdmin/Attendance/Configuration", values: new { area = "Staff" }));

                return Page();
            }
        }

        return RedirectToPage("/StudentAdmin/Attendance/Configuration", new { area = "Staff" });
    }

    private async Task PreparePage(CancellationToken cancellationToken)
    {
        StartDate = _dateTime.Today;

        _logger.Information("Requested to start creation of Student Absence Setting by user {User}", _currentUserService.UserName);

        Result<Dictionary<StudentId, string>> students = await _mediator.Send(new GetCurrentStudentsAsDictionaryQuery(), cancellationToken);

        if (students.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), students.Error, true)
                .Warning("Failed to start creation of Student Absence Setting by user {User}", _currentUserService.UserName);

            ModalContent = new ErrorDisplay(
                students.Error,
                _linkGenerator.GetPathByPage("/Partner/Students/Index", values: new { area = "Staff" }));

            return;
        }

        Students = new SelectList(students.Value, "Key", "Value");

        Result<List<SchoolSelectionListResponse>> schools = await _mediator.Send(new GetSchoolsForSelectionListQuery(GetSchoolsForSelectionListQuery.SchoolsFilter.PartnerSchools), cancellationToken);

        if (schools.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), schools.Error, true)
                .Warning("Failed to start creation of Student Absence Setting by user {User}", _currentUserService.UserName);

            ModalContent = new ErrorDisplay(
                schools.Error,
                _linkGenerator.GetPathByPage("/Partner/Students/Index", values: new { area = "Staff" }));

            return;
        }

        Schools = new SelectList(schools.Value, "Code", "Name");

        Grades = new SelectList(Enum.GetValues(typeof(Grade)).Cast<Grade>().Select(v => new { Text = v.GetDisplayName(), Value = ((int)v).ToString() }).ToList(), "Value", "Text");
    }
}
