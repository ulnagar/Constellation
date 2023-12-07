namespace Constellation.Presentation.Server.Areas.SchoolAdmin.Pages.Training.Roles;

using Application.Models.Auth;
using Application.Training.Models;
using Application.Training.Roles.CreateTrainingRole;
using Application.Training.Roles.GetTrainingRole;
using Application.Training.Roles.UpdateTrainingRole;
using BaseModels;
using Core.Models.Training.Identifiers;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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


    [ViewData] public string ActivePage { get; set; } = TrainingPages.Roles;
    [ViewData] public string StaffId { get; set; }

    public async Task OnGet()
    {
        StaffId = User.Claims.First(claim => claim.Type == AuthClaimType.StaffEmployeeId)?.Value;

        await GetClasses(_mediator);

        if (Id.HasValue)
        {
            TrainingRoleId roleId = TrainingRoleId.FromValue(Id.Value);

            Result<TrainingRoleResponse> request = await _mediator.Send(new GetTrainingRoleQuery(roleId));

            if (request.IsFailure)
            {
                Error = new()
                {
                    Error = request.Error,
                    RedirectPath = _linkGenerator.GetPathByPage("/Training/Roles/Index", values: new { area = "SchoolAdmin" })
                };

                return;
            }

            Name = request.Value.Name;
        }
    }

    public async Task<IActionResult> OnPostCreate()
    {
        StaffId = User.Claims.First(claim => claim.Type == AuthClaimType.StaffEmployeeId)?.Value;
        
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

        return RedirectToPage("/Training/Roles/Details", new { area = "SchoolAdmin", id = request.Value.Id.Value });
    }

    public async Task<IActionResult> OnPostUpdate()
    {
        StaffId = User.Claims.First(claim => claim.Type == AuthClaimType.StaffEmployeeId)?.Value;

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

        return RedirectToPage("/Training/Roles/Details", new { area = "SchoolAdmin", id = request.Value.Id.Value });
    }
}