namespace Constellation.Presentation.Shared.ViewComponents;

using Constellation.Application.Features.Common.Queries;
using Constellation.Application.Training.Models;
using Constellation.Application.Training.Roles.GetTrainingRole;
using Constellation.Core.Models.Training.Identifiers;
using Constellation.Core.Shared;
using Constellation.Presentation.Shared.Pages.Shared.Components.AddModuleToTrainingRole;
using MediatR;
using Microsoft.AspNetCore.Mvc;

public class AddModuleToTrainingRoleViewComponent : ViewComponent
{
    private readonly ISender _mediator;

    public AddModuleToTrainingRoleViewComponent(
        ISender mediator)
    {
        _mediator = mediator;
    }

    public async Task<IViewComponentResult> InvokeAsync(Guid id)
    {
        TrainingRoleId roleId = TrainingRoleId.FromValue(id);
        Result<TrainingRoleResponse> role = await _mediator.Send(new GetTrainingRoleQuery(roleId));
        Dictionary<Guid, string> modules = await _mediator.Send(new GetTrainingModulesAsDictionaryQuery());

        AddModuleToTrainingRoleSelection viewModel = new()
        {
            RoleId = roleId,
            RoleName = role.Value.Name,
            Modules = modules
        };

        return View(viewModel);
    }

}