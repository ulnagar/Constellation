namespace Constellation.Presentation.Server.ViewComponents;

using Constellation.Application.Models.Identity;
using Constellation.Presentation.Server.Pages.Shared.Components.RoleAddUser;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

public class RoleAddUserViewComponent : ViewComponent
{
    private readonly UserManager<AppUser> _userManager;
    private readonly RoleManager<AppRole> _roleManager;

    public RoleAddUserViewComponent(
        UserManager<AppUser> userManager,
        RoleManager<AppRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<IViewComponentResult> InvokeAsync(Guid RoleId)
    {
        var role = await _roleManager.FindByIdAsync(RoleId.ToString());

        var viewModel = new RoleAddUserSelection();
        viewModel.RoleId = RoleId;
        viewModel.RoleName = role.Name;
        viewModel.UserList = _userManager.Users
            .Select(user => 
                new RoleAddUserSelection.UserDto
                {
                    Id = user.Id,
                    Name = user.DisplayName,
                    Email = user.Email
                })
            .ToList();

        return View(viewModel);
    }
}
