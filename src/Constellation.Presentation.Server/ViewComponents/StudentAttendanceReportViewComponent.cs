namespace Constellation.Presentation.Server.ViewComponents;

using Application.Students.GetCurrentStudentsAsDictionary;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Pages.Shared.Components.StudentAttendanceReport;

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