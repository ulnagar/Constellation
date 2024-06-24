namespace Constellation.Presentation.Shared.ViewComponents;

using Application.Training.GetModuleDetails;
using Constellation.Application.Features.Common.Queries;
using Constellation.Application.Training.Models;
using Constellation.Core.Models.Training.Identifiers;
using Constellation.Core.Shared;
using Helpers.ModelBinders;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Pages.Shared.Components.AddStaffMemberToTrainingModule;

public class AddStaffMemberToTrainingModuleViewComponent : ViewComponent
{
    private readonly ISender _mediator;

    public AddStaffMemberToTrainingModuleViewComponent(
        ISender mediator)
    {
        _mediator = mediator;
    }

    public async Task<IViewComponentResult> InvokeAsync(
        [ModelBinder(typeof(StrongIdBinder))] TrainingModuleId moduleId)
    {
        Result<ModuleDetailsDto> module = await _mediator.Send(new GetModuleDetailsQuery(moduleId));

        if (module.IsFailure)
        {
            return Content(string.Empty);
        }

        Dictionary<string, string> staffResult = await _mediator.Send(new GetStaffMembersAsDictionaryQuery());

        AddStaffMemberToTrainingModuleSelection viewModel = new()
        {
            ModuleId = moduleId,
            ModuleName = module.Value.Name,
            StaffMembers = staffResult
        };

        return View(viewModel);
    }
}