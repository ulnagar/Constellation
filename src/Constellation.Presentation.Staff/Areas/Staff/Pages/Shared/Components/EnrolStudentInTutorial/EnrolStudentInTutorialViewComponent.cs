namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Shared.Components.EnrolStudentInTutorial;

using Constellation.Application.Domains.Students.Queries.GetCurrentStudentsAsDictionary;
using Constellation.Application.Domains.Tutorials.Queries.GetTutorialDetails;
using Constellation.Core.Models.Students.Identifiers;
using Constellation.Core.Models.Tutorials.Identifiers;
using Constellation.Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;

public class EnrolStudentInTutorialViewComponent : ViewComponent
{
    private readonly ISender _mediator;

    public EnrolStudentInTutorialViewComponent(
        ISender mediator)
    {
        _mediator = mediator;
    }

    public async Task<IViewComponentResult> InvokeAsync(
        TutorialId id)
    {
        Result<TutorialDetailsResponse> tutorial = await _mediator.Send(new GetTutorialDetailsQuery(id));
        Result<Dictionary<StudentId, string>> studentResult = await _mediator.Send(new GetCurrentStudentsAsDictionaryQuery());

        EnrolStudentInTutorialSelection viewModel = new()
        {
            TutorialId = id,
            TutorialName = tutorial.Value.Name,
            Students = studentResult.Value
        };

        return View(viewModel);
    }
}