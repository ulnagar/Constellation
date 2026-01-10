namespace Constellation.Presentation.Server.Areas.Admin.Pages.Auth.Roles;

using BaseModels;
using Constellation.Application.Common.PresentationModels;
using Constellation.Application.Domains.Auth.Commands.AddUserToRole;
using Constellation.Application.Domains.Auth.Commands.RemoveUserFromRole;
using Constellation.Application.Models.Auth;
using Constellation.Application.Models.Identity;
using Constellation.Core.Errors;
using Constellation.Presentation.Shared.Pages.Shared.Components.RoleAddUser;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

[Authorize(Policy = AuthPolicies.IsSiteAdmin)]
public class DetailsModel : BasePageModel
{
    private readonly IMediator _mediator;
    private readonly UserManager<AppUser> _userManager;
    private readonly RoleManager<AppRole> _roleManager;
    private readonly LinkGenerator _linkGenerator;

    public DetailsModel(
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

    [ViewData] public string ActivePage => Models.ActivePage.Auth_Roles;
    [ViewData] public string PageTitle => "Auth Roles";

    public List<UserRoleDetailsDto> Roles { get; set; } = new();

    public class UserRoleDetailsDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public int MemberCount { get; set; }
    }

    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    [BindProperty]
    public RoleAddUserSelection AddUserForm { get; set; }

    public string RoleName { get; set; }
    public List<RoleMemberDto> Members { get; set; } = new();

    public record RoleMemberDto(
        Guid UserId,
        string DisplayName,
        string EmailAddress);

    public async Task<IActionResult> OnGet()
    {
        AppRole role = await _roleManager.FindByIdAsync(Id.ToString());

        if (role is null)
            return Page();

        RoleName = role.Name;

        IList<AppUser> members = await _userManager.GetUsersInRoleAsync(RoleName);
        Members = members.Select(member =>
                new RoleMemberDto(
                    member.Id,
                    member.Name.DisplayName,
                    member.Email))
            .ToList();

        return Page();
    }

    public async Task<IActionResult> OnGetRemoveUser(Guid UserId)
    {
        if (UserId == Guid.Empty)
        {
            return ShowError(DomainErrors.Auth.UserNotFound);
        }

        var result = await _mediator.Send(new RemoveUserFromRoleCommand(Id, UserId));

        if (result.IsFailure)
        {
            return ShowError(result.Error);
        }

        return RedirectToPage();
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

        return RedirectToPage();
    }

    private IActionResult ShowError(Error error)
    {
        ModalContent = ErrorDisplay.Create(
            error,
            _linkGenerator.GetPathByPage("/Auth/Roles/Index", values: new { area = "Admin" }));

        AddUserForm = null;

        return Page();
    }
}
