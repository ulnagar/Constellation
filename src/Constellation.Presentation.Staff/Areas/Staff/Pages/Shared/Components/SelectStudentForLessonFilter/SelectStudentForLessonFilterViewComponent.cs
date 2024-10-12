namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Shared.Components.SelectStudentForLessonFilter;

using Constellation.Application.Students.GetCurrentStudentsAsDictionary;
using Constellation.Core.Shared;
using Core.Models.Students.Identifiers;
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
        Result<Dictionary<StudentId, string>> result = await _mediator.Send(new GetCurrentStudentsAsDictionaryQuery());

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