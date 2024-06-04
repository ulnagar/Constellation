namespace Constellation.Presentation.Staff.Areas.Staff.Pages.SchoolAdmin.Training.Roles;

using Application.Models.Auth;
using Application.StaffMembers.GetStaffList;
using Application.Training.Roles.GetTrainingRoleDetails;
using Constellation.Application.Training.Roles.AddStaffMemberToTrainingRole;
using Core.Models.Training.Identifiers;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

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

    [ViewData] public string ActivePage => Presentation.Staff.Pages.Shared.Components.StaffSidebarMenu.ActivePage.SchoolAdmin_Training_Roles;
   
    public async Task OnGet() => await PreparePage();

    public async Task PreparePage()
    {
        TrainingRoleId roleId = TrainingRoleId.FromValue(Id);

        Result<TrainingRoleDetailResponse> roleRequest = await _mediator.Send(new GetTrainingRoleDetailsQuery(roleId));

        if (roleRequest.IsFailure)
        {
            Error = new()
            {
                Error = roleRequest.Error,
                RedirectPath = _linkGenerator.GetPathByPage("/SchoolAdmin/Training/Roles/Details", values: new { area = "Staff", Id })
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
                RedirectPath = _linkGenerator.GetPathByPage("/SchoolAdmin/Training/Roles/Details", values: new { area = "Staff", Id })
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
            Error = new()
            {
                Error = request.Error,
                RedirectPath = null
            };

            return Page();
        }

        return RedirectToPage("/SchoolAdmin/Training/Roles/Details", new { area = "Staff", Id });
    }
}