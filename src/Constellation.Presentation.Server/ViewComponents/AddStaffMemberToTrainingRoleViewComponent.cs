namespace Constellation.Presentation.Server.ViewComponents;

using Application.Features.Common.Queries;
using Application.Training.Models;
using Application.Training.Roles.GetTrainingRole;
using Core.Models.Training.Identifiers;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Pages.Shared.Components.AddStaffMemberToTrainingRole;

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