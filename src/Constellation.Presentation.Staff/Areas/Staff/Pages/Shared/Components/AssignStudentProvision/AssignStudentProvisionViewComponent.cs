namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Shared.Components.AssignStudentProvision;

using Application.Domains.Assessments.Provisions.Models;
using Application.Domains.Assessments.Provisions.Queries.GetAssessmentProvisions;
using Application.Domains.Students.Queries.GetCurrentStudentsAsDictionary;
using Constellation.Core.Shared;
using Core.Models.Assessments.Identifiers;
using Core.Models.Students.Identifiers;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

public class AssignStudentProvisionViewComponent : ViewComponent
{
    private readonly ISender _mediator;

    public AssignStudentProvisionViewComponent(
        ISender mediator)
    {
        _mediator = mediator;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        Result<Dictionary<StudentId, string>> students = await _mediator.Send(new GetCurrentStudentsAsDictionaryQuery());

        if (students.IsFailure)
            return Content(string.Empty);

        Result<List<AssessmentProvisionResponse>> provisions = await _mediator.Send(new GetAssessmentProvisionsQuery());

        if (provisions.IsFailure)
            return Content(string.Empty);

        Dictionary<ProvisionId, string> provisionDictionary = provisions.Value
            .OrderBy(provision => provision.Code)
            .ToDictionary(k => k.Id, k => $"{k.Code}: {k.Description}");

        AssignStudentProvisionSelection viewModel = new()
        {
            StudentList = new SelectList(students.Value, "Key", "Value"),
            ProvisionList = new SelectList(provisionDictionary, "Key", "Value")
        };

        return View(viewModel);
    }
}