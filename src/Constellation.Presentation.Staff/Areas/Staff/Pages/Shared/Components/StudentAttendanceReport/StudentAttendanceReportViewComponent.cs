namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Shared.Components.StudentAttendanceReport;

using Application.Domains.Students.Queries.GetCurrentStudentsAsDictionary;
using Constellation.Core.Shared;
using Core.Models.Students.Identifiers;
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
        Result<Dictionary<StudentId, string>> result = await _mediator.Send(new GetCurrentStudentsAsDictionaryQuery());

        if (result.IsSuccess)
            viewModel.StudentList = result.Value;

        return View(viewModel);
    }
}