namespace Constellation.Presentation.Staff.Areas.Staff.Pages.SchoolAdmin.Training.Roles;

using Constellation.Application.Models.Auth;
using Constellation.Application.Training.Models;
using Constellation.Application.Training.Roles.CreateTrainingRole;
using Constellation.Application.Training.Roles.GetTrainingRole;
using Constellation.Application.Training.Roles.UpdateTrainingRole;
using Constellation.Core.Models.Training.Identifiers;
using Constellation.Core.Shared;
using Constellation.Presentation.Staff.Areas;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

[Authorize(Policy = AuthPolicies.CanEditTrainingModuleContent)]
public class CreateModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;

    public CreateModel(
        ISender mediator,
        LinkGenerator linkGenerator)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
    }

    [BindProperty(SupportsGet = true)]
    public Guid? Id { get; set; }

    [BindProperty]
    public string Name { get; set; }


    [ViewData] public string ActivePage => Presentation.Staff.Pages.Shared.Components.StaffSidebarMenu.ActivePage.SchoolAdmin_Training_Roles;

    public async Task OnGet()
    {
        if (Id.HasValue)
        {
            TrainingRoleId roleId = TrainingRoleId.FromValue(Id.Value);

            Result<TrainingRoleResponse> request = await _mediator.Send(new GetTrainingRoleQuery(roleId));

            if (request.IsFailure)
            {
                Error = new()
                {
                    Error = request.Error,
                    RedirectPath = _linkGenerator.GetPathByPage("/SchoolAdmin/Training/Roles/Index", values: new { area = "Staff" })
                };

                return;
            }

            Name = request.Value.Name;
        }
    }

    public async Task<IActionResult> OnPostCreate()
    {   
        if (!ModelState.IsValid)
        {
            return Page();
        }

        Result<TrainingRoleResponse> request = await _mediator.Send(new CreateTrainingRoleCommand(Name));

        if (request.IsFailure)
        {
            Error = new()
            {
                Error = request.Error,
                RedirectPath = null
            };

            return Page();
        }

        return RedirectToPage("/SchoolAdmin/Training/Roles/Details", new { area = "Staff", id = request.Value.Id.Value });
    }

    public async Task<IActionResult> OnPostUpdate()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        TrainingRoleId roleId = TrainingRoleId.FromValue(Id.Value);
        
        Result<TrainingRoleResponse> request = await _mediator.Send(new UpdateTrainingRoleCommand(roleId, Name));

        if (request.IsFailure)
        {
            Error = new()
            {
                Error = request.Error,
                RedirectPath = null
            };

            return Page();
        }

        return RedirectToPage("/SchoolAdmin/Training/Roles/Details", new { area = "Staff", id = request.Value.Id.Value });
    }
}