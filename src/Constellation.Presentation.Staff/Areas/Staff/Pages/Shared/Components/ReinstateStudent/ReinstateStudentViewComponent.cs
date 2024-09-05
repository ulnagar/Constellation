namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Shared.Components.ReinstateStudent;

using Application.Schools.GetSchoolsForSelectionList;
using Application.Schools.Models;
using Core.Models.Students.Identifiers;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;

public sealed class ReinstateStudentViewComponent : ViewComponent
{
    private readonly ISender _mediator;

    public ReinstateStudentViewComponent(
        ISender mediator)
    {
        _mediator = mediator;
    }

    public async Task<IViewComponentResult> InvokeAsync(StudentId studentId)
    {
        ReinstateStudentSelection viewModel = new();

        Result<List<SchoolSelectionListResponse>> schools = await _mediator.Send(new GetSchoolsForSelectionListQuery());

        if (schools.IsFailure)
            return Content(string.Empty);

        viewModel.Schools = schools.Value;

        return View(viewModel);
    }
}