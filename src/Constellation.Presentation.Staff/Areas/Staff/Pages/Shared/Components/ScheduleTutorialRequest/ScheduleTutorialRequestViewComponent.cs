namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Shared.Components.ScheduleTutorialRequest;

using Application.Domains.StaffMembers.Models;
using Application.Domains.StaffMembers.Queries.GetStaffForSelectionList;
using Application.Domains.Tutorials.Requests.Queries.GetTutorialRequestById;
using Core.Models.Tutorials.Identifiers;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

public sealed class ScheduleTutorialRequestViewComponent : ViewComponent
{
    private readonly ISender _mediator;

    public ScheduleTutorialRequestViewComponent(
        ISender mediator)
    {
        _mediator = mediator;
    }

    public async Task<IViewComponentResult> InvokeAsync(RequestId requestId)
    {
        Result<TutorialRequestDetailsResponse> request = await _mediator.Send(new GetTutorialRequestByIdQuery(requestId));

        if (request.IsFailure)
            return Content(string.Empty);

        ScheduleTutorialRequestSelection viewModel = new();

        foreach (var period in request.Value.Periods)
        {
            viewModel.Periods.Add(new(
                period.Id,
                period.ToString()));
        }

        Result<List<StaffSelectionListResponse>> staffMembers = await _mediator.Send(new GetStaffForSelectionListQuery());

        if (staffMembers.IsFailure)
            return Content(string.Empty);

        viewModel.StaffMembers = staffMembers.Value;

        return View(viewModel);
    }
}
