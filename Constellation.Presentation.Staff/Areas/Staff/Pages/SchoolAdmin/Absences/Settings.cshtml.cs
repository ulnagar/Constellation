namespace Constellation.Presentation.Staff.Areas.Staff.Pages.SchoolAdmin.Absences;

using Constellation.Application.Absences.SetAbsenceConfigurationForStudent;
using Constellation.Application.Helpers;
using Constellation.Application.Models.Auth;
using Constellation.Application.Schools.GetSchoolsForSelectionList;
using Constellation.Application.Students.GetCurrentStudentsAsDictionary;
using Constellation.Core.Enums;
using Constellation.Core.Models.Absences;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;

[Authorize(Policy = AuthPolicies.CanManageAbsences)]
public class SettingsModel : BasePageModel
{
    private readonly IMediator _mediator;
    private readonly LinkGenerator _linkGenerator;

    public SettingsModel(
        IMediator mediator,
        LinkGenerator linkGenerator)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
    }

    [ViewData] public string ActivePage => Presentation.Staff.Pages.Shared.Components.StaffSidebarMenu.ActivePage.SchoolAdmin_Absences_Audit;

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

    public async Task OnGet(CancellationToken cancellationToken = default)
    {
        var students = await _mediator.Send(new GetCurrentStudentsAsDictionaryQuery(), cancellationToken);

        if (students.IsFailure)
        {
            Error = new()
            {
                Error = students.Error,
                RedirectPath = _linkGenerator.GetPathByAction("Index", "Students", new { area = "Partner" })
            };

            return;
        }

        Students = new SelectList(students.Value, "Key", "Value");

        var schools = await _mediator.Send(new GetSchoolsForSelectionListQuery(), cancellationToken);

        if (schools.IsFailure)
        { 
            Error = new()
            {
                Error = schools.Error,
                RedirectPath = _linkGenerator.GetPathByAction("Index", "Students", new { area = "Partner" })
            };

            return;
        }

        Schools = new SelectList(schools.Value, "Code", "Name");

        Grades = new SelectList(Enum.GetValues(typeof(Grade)).Cast<Grade>().Select(v => new { Text = v.GetDisplayName(), Value = ((int)v).ToString() }).ToList(), "Value", "Text");

        return;
    }

    public async Task<IActionResult> OnPost(CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(StudentId) && string.IsNullOrWhiteSpace(SchoolCode))
        {
            Error = new()
            {
                Error = new("Validation.Page.EmptyValues", "You must select a value for either Student or School to continue"),
                RedirectPath = null
            };

            var students = await _mediator.Send(new GetCurrentStudentsAsDictionaryQuery(), cancellationToken);

            if (students.IsFailure)
            {
                Error = new()
                {
                    Error = students.Error,
                    RedirectPath = _linkGenerator.GetPathByAction("Index", "Students", new { area = "Partner" })
                };

                return Page();
            }

            Students = new SelectList(students.Value, "Key", "Value");

            var schools = await _mediator.Send(new GetSchoolsForSelectionListQuery(), cancellationToken);

            if (schools.IsFailure)
            {
                Error = new()
                {
                    Error = schools.Error,
                    RedirectPath = _linkGenerator.GetPathByAction("Index", "Students", new { area = "Partner" })
                };

                return Page();
            }

            Schools = new SelectList(schools.Value, "Code", "Name");

            Grades = new SelectList(Enum.GetValues(typeof(Grade)).Cast<Grade>().Select(v => new { Text = v.GetDisplayName(), Value = ((int)v).ToString() }).ToList(), "Value", "Text");

            return Page();
        }

        var request = await _mediator.Send(new SetAbsenceConfigurationForStudentCommand(
            StudentId, 
            SchoolCode, 
            Grade,
            AbsenceType.FromValue(Type),
            StartDate,
            EndDate), cancellationToken);

        if (request.IsFailure)
        {
            Error = new()
            {
                Error = request.Error,
                RedirectPath = _linkGenerator.GetPathByPage(page: "/SchoolAdmin/Absences/Audit", values: new { area = "Staff" })
            };

            return Page();
        }

        return RedirectToPage("/SchoolAdmin/Absences/Audit", new { area = "Staff" });
    }
}
