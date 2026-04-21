namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Shared.Components.AddStudentToAssessment;

using Application.Domains.Assessments.Assessments.Models;
using Application.Domains.Assessments.Assessments.Queries.GetAssessmentById;
using Application.Domains.Students.Queries.GetCurrentStudentsAsDictionary;
using Constellation.Core.Shared;
using Core.Models.Assessments.Identifiers;
using Core.Models.Students.Identifiers;
using MediatR;
using Microsoft.AspNetCore.Mvc;

public sealed class AddStudentToAssessmentViewComponent : ViewComponent
{
    private readonly ISender _mediator;

    public AddStudentToAssessmentViewComponent(
        ISender mediator)
    {
        _mediator = mediator;
    }

    public async Task<IViewComponentResult> InvokeAsync(AssessmentId id)
    {
        Result<AssessmentResponse> assessment = await _mediator.Send(new GetAssessmentByIdQuery(id));
        Result<Dictionary<StudentId, string>> studentResult = await _mediator.Send(new GetCurrentStudentsAsDictionaryQuery());

        AddStudentToAssessmentSelection viewModel = new(
            id,
            assessment.Value.Name,
            studentResult.Value);

        return View(viewModel);
    }
}
