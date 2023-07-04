namespace Constellation.Presentation.Server.Areas.SchoolAdmin.Pages.Absences;

using Constellation.Application.Absences.SetAbsenceConfigurationForStudent;
using Constellation.Application.Helpers;
using Constellation.Application.Schools.GetSchoolsForSelectionList;
using Constellation.Application.Students.GetCurrentStudentsAsDictionary;
using Constellation.Core.Enums;
using Constellation.Presentation.Server.BaseModels;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

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

    public SelectList Schools { get; set; }
    public SelectList Students { get; set; }
    public SelectList Grades { get; set; }

    [BindProperty(SupportsGet = false)]
    public string StudentId { get; set; }
    [BindProperty(SupportsGet = false)]
    public string SchoolCode { get; set; }
    [BindProperty(SupportsGet = false)]
    public int Grade { get; set; }

    public async Task OnGet(CancellationToken cancellationToken = default)
    {
        await GetClasses(_mediator);

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

            await GetClasses(_mediator);

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

        var request = await _mediator.Send(new SetAbsenceConfigurationForStudentCommand(StudentId, SchoolCode, Grade), cancellationToken);

        if (request.IsFailure)
        {
            Error = new()
            {
                Error = request.Error,
                RedirectPath = _linkGenerator.GetPathByAction("Index", "Students", new { area = "Partner" })
            };

            return Page();
        }

        return RedirectToAction("Index", "Students", new { area = "Partner" });
    }
}
