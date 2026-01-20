namespace Constellation.Presentation.Shared.Pages.Shared.Components.UserAddRole;

using Application.Models.Identity.Repositories;
using Constellation.Application.Models.Identity;
using Microsoft.AspNetCore.Mvc;

public class UserAddRoleViewComponent : ViewComponent
{
    private readonly IIdentityRepository _identityRepository;

    public UserAddRoleViewComponent(
        IIdentityRepository identityRepository)
    {
        _identityRepository = identityRepository;
    }

    public async Task<IViewComponentResult> InvokeAsync(Guid userId)
    {
        AppUser? user = await _identityRepository.GetUser(userId);

        if (user is null)
            return Content(string.Empty);

        List<AppRole> roles = await _identityRepository.GetRoles();
        List<AppRole> userRoles = await _identityRepository.GetRolesForUser(user);
        
        UserAddRoleSelection viewModel = new()
        { 
            Name = user.Name,
        };

        foreach (AppRole role in roles)
        {
            viewModel.RoleList.Add(new()
            {
                Id = role.Id,
                Name = role.Name,
                Type = role.Type,
                Available = !userRoles.Contains(role)
            });
        }

        return View(viewModel);
    }
}
