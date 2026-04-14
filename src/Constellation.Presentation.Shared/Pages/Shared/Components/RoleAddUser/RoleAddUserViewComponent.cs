namespace Constellation.Presentation.Shared.Pages.Shared.Components.RoleAddUser;

using Application.Models.Identity.Repositories;
using Constellation.Application.Models.Identity;
using Core.Models.Auth;
using Microsoft.AspNetCore.Mvc;

public class RoleAddUserViewComponent : ViewComponent
{
    private readonly IIdentityRepository _identityRepository;

    public RoleAddUserViewComponent(
        IIdentityRepository identityRepository)
    {
        _identityRepository = identityRepository;
    }

    public async Task<IViewComponentResult> InvokeAsync(Guid roleId)
    {
        AppRole? role = await _identityRepository.GetRole(roleId);

        if (role is null)
            return Content(string.Empty);

        List<AppUser> users = await _identityRepository.GetUsers();
        List<AppUser> roleUsers = await _identityRepository.GetUsersInRole(role.Name);
        
        RoleAddUserSelection viewModel = new()
        { 
            RoleName = role.Name,
        };

        foreach (AppUser user in users)
        {
            viewModel.UserList.Add(new()
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Available = !roleUsers.Contains(user)
            });
        }

        return View(viewModel);
    }
}
