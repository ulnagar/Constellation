namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Shared.Components.SelectSchoolForLessonFilter;

using Application.Domains.Schools.Models;
using Application.Domains.Schools.Queries.GetCurrentPartnerSchoolsWithStudentsList;
using Constellation.Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;

public class SelectSchoolForLessonFilterViewComponent : ViewComponent
{
    private readonly ISender _mediator;

    public SelectSchoolForLessonFilterViewComponent(
        ISender mediator)
    {
        _mediator = mediator;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        SelectSchoolForLessonFilterSelection viewModel = new();
        Result<List<SchoolSelectionListResponse>> result = await _mediator.Send(new GetCurrentPartnerSchoolsWithStudentsListQuery());

        if (result.IsFailure)
        {
            // How? This method does not return failure
        }
        else
        {
            viewModel.SchoolList = result.Value.ToDictionary(k => k.Code, k => k.Name);
        }

        return View(viewModel);
    }
}