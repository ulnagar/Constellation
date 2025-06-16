namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Shared.Components.ReinstateStaffMember;

using Constellation.Application.Domains.Schools.Models;
using Constellation.Application.Domains.Schools.Queries.GetSchoolsForSelectionList;
using Constellation.Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

public sealed class ReinstateStaffMemberViewComponent : ViewComponent
{
    private readonly ISender _mediator;

    public ReinstateStaffMemberViewComponent(
        ISender mediator)
    {
        _mediator = mediator;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        Result<List<SchoolSelectionListResponse>> schools = await _mediator.Send(new GetSchoolsForSelectionListQuery());

        if (schools.IsFailure)
            return Content(string.Empty);

        ReinstateStaffMemberSelection viewModel = new()
        {
            SchoolList = new SelectList(schools.Value.OrderBy(entry => entry.Name), "Code", "Name")
        };

        return View(viewModel);
    }
}