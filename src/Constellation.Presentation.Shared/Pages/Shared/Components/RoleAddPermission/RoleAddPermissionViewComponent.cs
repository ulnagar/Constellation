namespace Constellation.Presentation.Shared.Pages.Shared.Components.RoleAddUPermission;

using Application.Models.Auth;
using Application.Models.Identity.Repositories;
using Microsoft.AspNetCore.Mvc;

public class RoleAddPermissionViewComponent : ViewComponent
{
    private readonly IIdentityRepository _identityRepository;

    public RoleAddPermissionViewComponent(
        IIdentityRepository identityRepository)
    {
        _identityRepository = identityRepository;
    }

    public async Task<IViewComponentResult> InvokeAsync(Guid roleId)
    {
        List<AuthPermission> enabledPermissions = await _identityRepository.GetRolePermissions(roleId);

        IEnumerable<AuthPermission> permissions = AuthPermission.GetOptions;

        RoleAddPermissionSelection viewModel = new RoleAddPermissionSelection();
        foreach (var permission in permissions)
        {
            if (enabledPermissions.Contains(permission))
            {
                viewModel.Permissions.Add(new(permission, false));
            }
            else
            {
                viewModel.Permissions.Add(new(permission, true));
            }
        }

        return View(viewModel);
    }
}