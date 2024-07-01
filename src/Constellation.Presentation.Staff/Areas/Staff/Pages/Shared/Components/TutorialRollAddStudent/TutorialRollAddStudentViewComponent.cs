namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Shared.Components.TutorialRollAddStudent;

using Constellation.Application.Students.GetCurrentStudentsAsDictionary;
using MediatR;
using Microsoft.AspNetCore.Mvc;

public class TutorialRollAddStudentViewComponent : ViewComponent
{
    private readonly IMediator _mediator;

    public TutorialRollAddStudentViewComponent(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var viewModel = new TutorialRollAddStudentSelection();
        var result = await _mediator.Send(new GetCurrentStudentsAsDictionaryQuery());

        if (result.IsFailure)
        {
            // How? This method does not return failure
        }
        else
        {
            viewModel.StudentList = result.Value;
        }

        return View(viewModel);
    }
}
