namespace Constellation.Presentation.Shared.ViewComponents;

using Constellation.Application.Students.GetCurrentStudentsAsDictionary;
using Constellation.Core.Shared;
using Constellation.Presentation.Shared.Pages.Shared.Components.SelectStudentForLessonFilter;
using MediatR;
using Microsoft.AspNetCore.Mvc;

public class SelectStudentForLessonFilterViewComponent : ViewComponent
{
    private readonly ISender _mediator;

    public SelectStudentForLessonFilterViewComponent(
        ISender mediator)
    {
        _mediator = mediator;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        SelectStudentForLessonFilterSelection viewModel = new();
        Result<Dictionary<string, string>> result = await _mediator.Send(new GetCurrentStudentsAsDictionaryQuery());

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