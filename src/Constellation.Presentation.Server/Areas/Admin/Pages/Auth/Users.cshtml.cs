namespace Constellation.Presentation.Server.Areas.Admin.Pages.Auth;

using Constellation.Application.Models.Auth;
using Constellation.Application.Models.Identity;
using Constellation.Presentation.Server.BaseModels;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

[Authorize(Policy = AuthPolicies.IsSiteAdmin)]
public class UsersModel : BasePageModel
{
    private readonly IMediator _mediator;
    private readonly UserManager<AppUser> _userManager;

    public UsersModel(IMediator mediator, UserManager<AppUser> userManager)
    {
        _mediator = mediator;
        _userManager = userManager;
    }

    [BindProperty(SupportsGet = true)]
    public UserType SelectedUserType { get; set; } = UserType.Staff;

    public List<AppUser> Users { get; set; } = new();

    public enum UserType
    {
        Staff,
        School,
        Parent
    }

    public async Task OnGet()
    {
        Users = SelectedUserType switch
        {
            UserType.Staff => _userManager.Users.Where(user => user.IsStaffMember).ToList(),
            UserType.School => _userManager.Users.Where(user => user.IsSchoolContact).ToList(),
            UserType.Parent => _userManager.Users.Where(user => user.IsParent).ToList(),
            _ => _userManager.Users.Where(user => user.IsStaffMember).ToList()
        };
    }
}
