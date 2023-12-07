namespace Constellation.Presentation.Server.Areas.SchoolAdmin.Pages.Training.Roles;

using Application.Models.Auth;
using Application.Training.Roles.AddStaffMemberToTrainingRole;
using Application.Training.Roles.GetTrainingRoleDetails;
using Application.Training.Roles.RemoveStaffMemberFromTrainingRole;
using BaseModels;
using Core.Models.Training.Identifiers;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Server.Pages.Shared.Components.AddStaffMemberToTrainingRole;

[Authorize(Policy = AuthPolicies.IsStaffMember)]
public class DetailsModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;

    public DetailsModel(
        ISender mediator,
        LinkGenerator linkGenerator)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
    }
    
    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    public TrainingRoleDetailResponse Role { get; set; }

    [ViewData] public string ActivePage { get; set; } = TrainingPages.Roles;
    [ViewData] public string StaffId {get; set; }

    public async Task OnGet()
    {
        StaffId = User.Claims.First(claim => claim.Type == AuthClaimType.StaffEmployeeId)?.Value;

        await GetClasses(_mediator);

        TrainingRoleId roleId = TrainingRoleId.FromValue(Id);

        Result<TrainingRoleDetailResponse> request = await _mediator.Send(new GetTrainingRoleDetailsQuery(roleId));

        if (request.IsFailure)
        {
            Error = new()
            {
                Error = request.Error,
                RedirectPath = _linkGenerator.GetPathByPage("/Training/Roles/Index", values: new { area = "SchoolAdmin" })
            };

            return;
        }

        Role = request.Value;
    }

    public async Task<IActionResult> OnGetRemoveStaff(string staffId)
    {
        TrainingRoleId roleId = TrainingRoleId.FromValue(Id);

        Result request = await _mediator.Send(new RemoveStaffMemberFromTrainingRoleCommand(roleId, staffId));

        if (request.IsFailure)
        {
            StaffId = User.Claims.First(claim => claim.Type == AuthClaimType.StaffEmployeeId)?.Value;
            await GetClasses(_mediator);

            Error = new()
            {
                Error = request.Error,
                RedirectPath = null
            };

            return Page();
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostAddStaffMember(AddStaffMemberToTrainingRoleSelection viewModel)
    {
        StaffId = User.Claims.First(claim => claim.Type == AuthClaimType.StaffEmployeeId)?.Value;

        await GetClasses(_mediator);

        if (string.IsNullOrWhiteSpace(viewModel.StaffId))
        {
            Error = new()
            {
                Error = Core.Shared.Error.NullValue,
                RedirectPath = null
            };

            return Page();
        }

        TrainingRoleId roleId = TrainingRoleId.FromValue(Id);

        Result request = await _mediator.Send(new AddStaffMemberToTrainingRoleCommand(roleId, viewModel.StaffId));

        if (request.IsFailure)
        {
            Error = new()
            {
                Error = request.Error,
                RedirectPath = null
            };

            return Page();
        }

        return RedirectToPage();
    }
}