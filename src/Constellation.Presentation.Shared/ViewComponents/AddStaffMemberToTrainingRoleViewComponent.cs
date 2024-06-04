namespace Constellation.Presentation.Shared.ViewComponents;

using Constellation.Application.Features.Common.Queries;
using Constellation.Application.Training.Models;
using Constellation.Application.Training.Roles.GetTrainingRole;
using Constellation.Core.Models.Training.Identifiers;
using Constellation.Core.Shared;
using Constellation.Presentation.Shared.Pages.Shared.Components.AddStaffMemberToTrainingRole;
using MediatR;
using Microsoft.AspNetCore.Mvc;

public class AddStaffMemberToTrainingRoleViewComponent : ViewComponent
{
    private readonly ISender _mediator;

    public AddStaffMemberToTrainingRoleViewComponent(
        ISender mediator)
    {
        _mediator = mediator;
    }

    public async Task<IViewComponentResult> InvokeAsync(Guid id)
    {
        TrainingRoleId roleId = TrainingRoleId.FromValue(id);
        Result<TrainingRoleResponse> role = await _mediator.Send(new GetTrainingRoleQuery(roleId));
        Dictionary<string, string> staffResult = await _mediator.Send(new GetStaffMembersAsDictionaryQuery());

        AddStaffMemberToTrainingRoleSelection viewModel = new()
        {
            RoleId = roleId,
            RoleName = role.Value.Name,
            StaffMembers = staffResult
        };

        return View(viewModel);
    }
}