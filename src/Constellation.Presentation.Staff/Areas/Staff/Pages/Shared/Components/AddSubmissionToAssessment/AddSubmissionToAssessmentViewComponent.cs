namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Shared.Components.AddSubmissionToAssessment;

using Application.Domains.Assessments.Assessments.Queries.GetStudentsFromAssessment;
using Core.Models.Assessments.Identifiers;
using Core.Models.Students;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;

public sealed class AddSubmissionToAssessmentViewComponent : ViewComponent
{
    private readonly ISender _mediator;

    public AddSubmissionToAssessmentViewComponent(ISender mediator)
    {
        _mediator = mediator;
    }

    public async Task<IViewComponentResult> InvokeAsync(AssessmentId AssessmentId)
    {
        Result<List<Student>> result = await _mediator.Send(new GetStudentsFromAssessmentQuery(AssessmentId));

        if (result.IsFailure)
        {
            return Content(string.Empty);
        }

        AssessmentSubmissionSelection viewModel = new()
        {
            StudentList = result.Value.OrderBy(student => student.Name.SortOrder).ToDictionary(student => student.Id, student => student.Name.DisplayName)
        };

        return View(viewModel);
    }
}
