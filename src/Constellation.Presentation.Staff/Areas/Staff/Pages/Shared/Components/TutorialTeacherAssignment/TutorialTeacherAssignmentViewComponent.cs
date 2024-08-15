namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Shared.Components.TutorialTeacherAssignment;

using Constellation.Application.Features.Common.Queries;
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
        viewModel.StaffList = await _mediator.Send(new GetStaffMembersAsDictionaryQuery());

        return View(viewModel);
    }
}
