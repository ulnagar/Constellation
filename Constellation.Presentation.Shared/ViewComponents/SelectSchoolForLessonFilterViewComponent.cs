namespace Constellation.Presentation.Shared.ViewComponents;

using Constellation.Application.Schools.GetCurrentPartnerSchoolsWithStudentsList;
using Constellation.Application.Schools.Models;
using Constellation.Core.Shared;
using Constellation.Presentation.Shared.Pages.Shared.Components.SelectSchoolForLessonFilter;
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