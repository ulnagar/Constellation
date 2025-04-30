namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Shared.Components.TutorialTeacherAssignment;

using Constellation.Application.StaffMembers.GetStaffMembersAsDictionary;
using Constellation.Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;

public class TutorialTeacherAssignmentViewComponent : ViewComponent
{
    private readonly ISender _mediator;

    public TutorialTeacherAssignmentViewComponent(ISender mediator)
    {
        _mediator = mediator;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var viewModel = new TutorialTeacherAssignmentSelection();

        Result<Dictionary<string, string>> staffListRequest = await _mediator.Send(new GetStaffMembersAsDictionaryQuery());

        if (staffListRequest.IsSuccess)
        {
            viewModel.StaffList = staffListRequest.Value;
        }

        return View(viewModel);
    }
}
