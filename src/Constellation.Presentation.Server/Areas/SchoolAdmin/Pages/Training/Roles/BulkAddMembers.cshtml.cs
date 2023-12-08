namespace Constellation.Presentation.Server.Areas.SchoolAdmin.Pages.Training.Roles;

using Application.Models.Auth;
using Application.StaffMembers.GetStaffList;
using Application.Training.Roles.GetTrainingRoleDetails;
using BaseModels;
using Constellation.Application.Training.Roles.AddStaffMemberToTrainingRole;
using Core.Models.Training.Identifiers;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize(Policy = AuthPolicies.CanEditTrainingModuleContent)]
public class BulkAddMembersModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;

    public BulkAddMembersModel(
        ISender mediator,
        LinkGenerator linkGenerator)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
    }

    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    public TrainingRoleDetailResponse Role { get; set; }
    public List<StaffResponse> StaffMembers { get; set; } = new();

    [BindProperty]
    public List<string> SelectedStaffIds { get; set; } = new();

    [ViewData] public string ActivePage { get; set; } = TrainingPages.Roles;
    [ViewData] public string StaffId { get; set; }
    
    public async Task OnGet() => await PreparePage();

    public async Task PreparePage()
    {
        StaffId = User.Claims.First(claim => claim.Type == AuthClaimType.StaffEmployeeId)?.Value;

        await GetClasses(_mediator);

        TrainingRoleId roleId = TrainingRoleId.FromValue(Id);

        Result<TrainingRoleDetailResponse> roleRequest = await _mediator.Send(new GetTrainingRoleDetailsQuery(roleId));

        if (roleRequest.IsFailure)
        {
            Error = new()
            {
                Error = roleRequest.Error,
                RedirectPath = _linkGenerator.GetPathByPage("/Training/Roles/Details", values: new { area = "SchoolAdmin", Id })
            };

            return;
        }

        Role = roleRequest.Value;

        Result<List<StaffResponse>> staffRequest = await _mediator.Send(new GetStaffListQuery());

        if (staffRequest.IsFailure)
        {
            Error = new()
            {
                Error = staffRequest.Error,
                RedirectPath = _linkGenerator.GetPathByPage("/Training/Roles/Details", values: new { area = "SchoolAdmin", Id })
            };

            return;
        }

        StaffMembers = staffRequest.Value.Where(member => !member.IsDeleted).ToList();
    }

    public async Task<IActionResult> OnPost()
    {
        if (SelectedStaffIds.Count == 0)
        {
            ModelState.AddModelError("", "You must select at least one Staff Member to add");

            await PreparePage();

            return Page();
        }

        TrainingRoleId roleId = TrainingRoleId.FromValue(Id);

        Result request = await _mediator.Send(new AddStaffMemberToTrainingRoleCommand(roleId, SelectedStaffIds));

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

        return RedirectToPage("/Training/Roles/Details", new { area = "SchoolAdmin", Id });
    }
}