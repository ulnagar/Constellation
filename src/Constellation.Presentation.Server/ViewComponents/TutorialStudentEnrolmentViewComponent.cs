namespace Constellation.Presentation.Server.ViewComponents;

using Constellation.Application.Students.GetCurrentStudentsAsDictionary;
using Constellation.Presentation.Server.Pages.Shared.Components.TutorialStudentEnrolment;
using MediatR;
using Microsoft.AspNetCore.Mvc;

public class TutorialStudentEnrolmentViewComponent : ViewComponent
{
    private readonly IMediator _mediator;

    public TutorialStudentEnrolmentViewComponent(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var viewModel = new TutorialStudentEnrolmentSelection();
        var result =  await _mediator.Send(new GetCurrentStudentsAsDictionaryQuery());

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