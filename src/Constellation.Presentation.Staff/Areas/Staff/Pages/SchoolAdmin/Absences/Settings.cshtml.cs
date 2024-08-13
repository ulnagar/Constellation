namespace Constellation.Presentation.Staff.Areas.Staff.Pages.SchoolAdmin.Absences;

using Application.Common.PresentationModels;
using Application.Schools.Models;
using Constellation.Application.Absences.SetAbsenceConfigurationForStudent;
using Constellation.Application.Helpers;
using Constellation.Application.Models.Auth;
using Constellation.Application.Schools.GetSchoolsForSelectionList;
using Constellation.Application.Students.GetCurrentStudentsAsDictionary;
using Constellation.Core.Enums;
using Constellation.Core.Models.Absences;
using Core.Abstractions.Services;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using Models;
using Serilog;
using System.Threading;

[Authorize(Policy = AuthPolicies.CanManageAbsences)]
public class SettingsModel : BasePageModel
{
    private readonly IMediator _mediator;
    private readonly LinkGenerator _linkGenerator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public SettingsModel(
        IMediator mediator,
        LinkGenerator linkGenerator,
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
        _currentUserService = currentUserService;
        _logger = logger
            .ForContext<SettingsModel>()
            .ForContext(StaffLogDefaults.Application, StaffLogDefaults.StaffPortal);
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.SchoolAdmin_Absences_Audit;
    [ViewData] public string PageTitle => "Student Absence Settings";

    public SelectList Schools { get; set; }
    public SelectList Students { get; set; }
    public SelectList Grades { get; set; }

    [BindProperty(SupportsGet = false)]
    public string StudentId { get; set; }
    [BindProperty(SupportsGet = false)]
    public string SchoolCode { get; set; }
    [BindProperty(SupportsGet = false)]
    public int Grade { get; set; }

    [BindProperty]
    public string Type { get; set; }
    [BindProperty]
    public DateOnly StartDate { get; set; }
    [BindProperty]
    public DateOnly? EndDate { get; set; }

    public async Task OnGet(CancellationToken cancellationToken = default) => await PreparePage(cancellationToken);

    public async Task<IActionResult> OnPost(CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(StudentId) && string.IsNullOrWhiteSpace(SchoolCode))
        {
            Error error = new("Validation.Page.EmptyValues", "You must select a value for either Student or School to continue");

            _logger
                .ForContext(nameof(Error), error, true)
                .Warning("Failed to create new Student Absence Setting by user {User}", _currentUserService.UserName);

            ModalContent = new ErrorDisplay(error);

            await PreparePage(cancellationToken);

            return Page();
        }

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
                _linkGenerator.GetPathByPage(page: "/SchoolAdmin/Absences/Audit", values: new { area = "Staff" }));

            return Page();
        }

        return RedirectToPage("/SchoolAdmin/Absences/Audit", new { area = "Staff" });
    }

    private async Task PreparePage(CancellationToken cancellationToken)
    {
        _logger.Information("Requested to start creation of Student Absence Setting by user {User}", _currentUserService.UserName);

        Result<Dictionary<string, string>> students = await _mediator.Send(new GetCurrentStudentsAsDictionaryQuery(), cancellationToken);

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

        Result<List<SchoolSelectionListResponse>> schools = await _mediator.Send(new GetSchoolsForSelectionListQuery(), cancellationToken);

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
