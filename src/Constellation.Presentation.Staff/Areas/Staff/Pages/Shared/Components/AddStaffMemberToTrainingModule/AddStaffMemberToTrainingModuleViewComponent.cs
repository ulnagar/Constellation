namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Shared.Components.AddStaffMemberToTrainingModule;

using Constellation.Application.StaffMembers.GetStaffMembersAsDictionary;
using Constellation.Application.Training.GetModuleDetails;
using Constellation.Application.Training.Models;
using Constellation.Core.Models.Training.Identifiers;
using Constellation.Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

public class AddStaffMemberToTrainingModuleViewComponent : ViewComponent
{
    private readonly ISender _mediator;

    public AddStaffMemberToTrainingModuleViewComponent(
        ISender mediator)
    {
        _mediator = mediator;
    }

    public async Task<IViewComponentResult> InvokeAsync(TrainingModuleId moduleId)
    {
        Result<ModuleDetailsDto> module = await _mediator.Send(new GetModuleDetailsQuery(moduleId));

        if (module.IsFailure)
            return Content(string.Empty);
        
        Result<Dictionary<string, string>> staffListRequest = await _mediator.Send(new GetStaffMembersAsDictionaryQuery());

        if (staffListRequest.IsFailure)
        {
            return Content(string.Empty);
        }

        Dictionary<string, string> staffResult = staffListRequest.Value;

        foreach (ModuleDetailsDto.Assignee assignee in module.Value.Assignees)
            staffResult.Remove(assignee.StaffId);

        AddStaffMemberToTrainingModuleSelection viewModel = new()
        {
            ModuleId = moduleId,
            ModuleName = module.Value.Name,
            StaffMembers = staffResult
        };

        return View(viewModel);
    }
}