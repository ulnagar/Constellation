namespace Constellation.Presentation.Server.Areas.Admin.Pages.Auth;

using Application.Common.PresentationModels;
using Constellation.Application.AdminDashboards.AddUserToRole;
using Constellation.Application.AdminDashboards.RemoveUserFromRole;
using Constellation.Application.Models.Auth;
using Constellation.Application.Models.Identity;
using Constellation.Core.Errors;
using Constellation.Core.Shared;
using Constellation.Presentation.Server.BaseModels;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Presentation.Shared.Pages.Shared.Components.RoleAddUser;

[Authorize(Policy = AuthPolicies.IsSiteAdmin)]
public class RoleModel : BasePageModel
{
    private readonly IMediator _mediator;
    private readonly UserManager<AppUser> _userManager;
    private readonly RoleManager<AppRole> _roleManager;
    private readonly LinkGenerator _linkGenerator;

    public RoleModel(
        IMediator mediator,
        UserManager<AppUser> userManager,
        RoleManager<AppRole> roleManager,
        LinkGenerator linkGenerator)
    {
        _mediator = mediator;
        _userManager = userManager;
        _roleManager = roleManager;
        _linkGenerator = linkGenerator;
    }

    [BindProperty(SupportsGet = true)]
    public Guid RoleId { get; set; }

    [BindProperty]
    public RoleAddUserSelection AddUserForm { get; set; }

    public string RoleName { get; set; }
    public List<RoleMemberDto> Members { get; set; } = new();

    public class RoleMemberDto
    {
        public Guid UserId { get; set; }
        public string DisplayName { get; set; }
        public string EmailAddress { get; set; }
    }

    public async Task<IActionResult> OnGet()
    {
        var role = await _roleManager.FindByIdAsync(RoleId.ToString());

        if (role is null)
        {
            return RedirectToPage("Index");
        }

        RoleName = role.Name;

        var members = await _userManager.GetUsersInRoleAsync(RoleName);
        Members = members.Select(member => 
            new RoleMemberDto
                {
                    UserId = member.Id,
                    DisplayName = member.DisplayName,
                    EmailAddress = member.Email
                })
            .ToList();

        return Page();
    }

    public async Task<IActionResult> OnGetRemoveUser(Guid UserId)
    {
        if (UserId == Guid.Empty)
        {
            return ShowError(DomainErrors.Auth.UserNotFound);
        }

        var result = await _mediator.Send(new RemoveUserFromRoleCommand(RoleId, UserId));

        if (result.IsFailure)
        {
            return ShowError(result.Error);
        }

        return RedirectToPage("Role");
    }

    public async Task<IActionResult> OnPostAddUser()
    {
        if (AddUserForm.UserId == Guid.Empty)
        {
            return ShowError(DomainErrors.Auth.UserNotFound);
        }

        var result = await _mediator.Send(new AddUserToRoleCommand(AddUserForm.RoleId, AddUserForm.UserId));
        
        if (result.IsFailure)
        {
            return ShowError(result.Error);
        }

        return RedirectToPage("Role");
    }

    private IActionResult ShowError(Error error)
    {
        Error = new ErrorDisplay
        {
            Error = error,
            RedirectPath = _linkGenerator.GetPathByPage("/Auth/Role", values: new { area = "Admin", RoleId = RoleId })
        };

        AddUserForm = null;

        return Page();
    }
}
