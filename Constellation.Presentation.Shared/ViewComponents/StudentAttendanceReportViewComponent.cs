namespace Constellation.Presentation.Shared.ViewComponents;

using Constellation.Application.Students.GetCurrentStudentsAsDictionary;
using Constellation.Core.Shared;
using Constellation.Presentation.Shared.Pages.Shared.Components.StudentAttendanceReport;
using MediatR;
using Microsoft.AspNetCore.Mvc;

public class StudentAttendanceReportViewComponent : ViewComponent
{
    private readonly ISender _mediator;

    public StudentAttendanceReportViewComponent(
        ISender mediator)
    {
        _mediator = mediator;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        AttendanceReportSelection viewModel = new();
        Result<Dictionary<string, string>> result = await _mediator.Send(new GetCurrentStudentsAsDictionaryQuery());

        if (result.IsSuccess)
        {
            viewModel.StudentList = result.Value;
        }

        return View(viewModel);
    }
}