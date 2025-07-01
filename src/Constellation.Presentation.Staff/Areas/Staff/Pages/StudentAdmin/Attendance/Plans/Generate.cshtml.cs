namespace Constellation.Presentation.Staff.Areas.Staff.Pages.StudentAdmin.Attendance.Plans;

using Application.Domains.Schools.Models;
using Application.Domains.Schools.Queries.GetSchoolsForSelectionList;
using Application.Domains.Students.Queries.GetCurrentStudentsAsDictionary;
using Application.Extensions;
using Application.Helpers;
using Constellation.Application.Common.PresentationModels;
using Constellation.Application.Domains.Attendance.Plans.Commands.GenerateAttendancePlans;
using Constellation.Application.Models.Auth;
using Constellation.Core.Enums;
using Constellation.Core.Models.Students.Identifiers;
using Constellation.Core.Shared;
using Core.Abstractions.Clock;
using Core.Abstractions.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using Presentation.Shared.Helpers.Logging;
using Serilog;

[Authorize(Policy = AuthPolicies.CanManageAbsences)]
public class GenerateModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeProvider _dateTime;
    private readonly ILogger _logger;

    public GenerateModel(
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

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.StudentAdmin_Attendance_Plans;
    [ViewData] public string PageTitle => "Attendance Plan Generator";

    public SelectList Schools { get; set; }
    public SelectList Students { get; set; }
    public SelectList Grades { get; set; }

    [BindProperty]
    public StudentId StudentId { get; set; } = StudentId.Empty;
    [BindProperty]
    public string? SchoolCode { get; set; }
    [BindProperty]
    public int? Grade { get; set; }

    public async Task OnGet(CancellationToken cancellationToken) => await PreparePage(cancellationToken);

    private async Task PreparePage(CancellationToken cancellationToken)
    {
        _logger.Information("Requested to start creation of Student Absence Plans by user {User}", _currentUserService.UserName);

        Result<Dictionary<StudentId, string>> students = await _mediator.Send(new GetCurrentStudentsAsDictionaryQuery(), cancellationToken);

        if (students.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), students.Error, true)
                .Warning("Failed to start creation of Student Absence Plans by user {User}", _currentUserService.UserName);

            ModalContent = ErrorDisplay.Create(
                students.Error,
                _linkGenerator.GetPathByPage("/StudentAdmin/Attendance/Plans/Index", values: new { area = "Staff" }));

            return;
        }

        Students = new SelectList(students.Value, "Key", "Value");

        Result<List<SchoolSelectionListResponse>> schools = await _mediator.Send(new GetSchoolsForSelectionListQuery(GetSchoolsForSelectionListQuery.SchoolsFilter.PartnerSchools), cancellationToken);

        if (schools.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), schools.Error, true)
                .Warning("Failed to start creation of Student Absence Plans by user {User}", _currentUserService.UserName);

            ModalContent = ErrorDisplay.Create(
                schools.Error,
                _linkGenerator.GetPathByPage("/StudentAdmin/Attendance/Plans/Index", values: new { area = "Staff" }));

            return;
        }

        Schools = new SelectList(schools.Value.OrderBy(entry => entry.Name), nameof(SchoolSelectionListResponse.Code), nameof(SchoolSelectionListResponse.Name));

        Grades = new SelectList(Enum.GetValues<Grade>().Select(v => new { Text = v.GetDisplayName(), Value = ((int)v).ToString() }).ToList(), "Value", "Text");
    }

    public async Task<IActionResult> OnPost(CancellationToken cancellationToken = default)
    {
        if (StudentId == StudentId.Empty && string.IsNullOrWhiteSpace(SchoolCode) && !Grade.HasValue)
        {
            Error error = new("Validation.Page.EmptyValues", "You must select a value for Student, Grade, or School to continue");

            _logger
                .ForContext(nameof(Error), error, true)
                .Warning("Failed to create new Student Attendance Plan by user {User}", _currentUserService.UserName);

            ModalContent = ErrorDisplay.Create(error);

            await PreparePage(cancellationToken);

            return Page();
        }

        Grade? grade = Grade switch
        {
            5 => Core.Enums.Grade.Y05,
            6 => Core.Enums.Grade.Y06,
            7 => Core.Enums.Grade.Y07,
            8 => Core.Enums.Grade.Y08,
            9 => Core.Enums.Grade.Y09,
            10 => Core.Enums.Grade.Y10,
            11 => Core.Enums.Grade.Y11,
            12 => Core.Enums.Grade.Y12,
            _ => null
        };

        GenerateAttendancePlansCommand command = new(
            StudentId,
            SchoolCode,
            grade);

        _logger
            .ForContext(nameof(GenerateAttendancePlansCommand), command, true)
            .Information("Requested to create new Student Attendance Plan by user {User}", _currentUserService.UserName);

        Result request = await _mediator.Send(command, cancellationToken);

        if (request.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), request.Error, true)
                .Warning("Failed to create new Student Attendance Plan by user {User}", _currentUserService.UserName);

            ModalContent = ErrorDisplay.Create(
                request.Error,
                _linkGenerator.GetPathByPage(page: "/StudentAdmin/Attendance/Plans/Index", values: new { area = "Staff" }));

            return Page();
        }

        return RedirectToPage("/StudentAdmin/Attendance/Plans/Index", new { area = "Staff" });
    }
}