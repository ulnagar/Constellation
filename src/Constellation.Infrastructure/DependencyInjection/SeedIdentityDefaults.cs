namespace Constellation.Infrastructure.DependencyInjection;

using Application.Models.Identity.Enums;
using Constellation.Application.Models.Auth;
using Constellation.Application.Models.Identity;
using Core.Models.SchoolContacts.Enums;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

public static class IdentityDefaults
{
    public static string SuperAdminRole = "SuperAdmin";

    public static async Task SeedRoles(RoleManager<AppRole> roleManager)
    {
        List<AuthPermission> permissions = AuthPermission.GetOptions.ToList();

        await CreateRoleWithPermission(roleManager, SuperAdminRole, permissions, AppRoleType.Staff);

        IEnumerable<Position> positions = Position.GetOptions;

        foreach (var position in positions)
            await CreateRole(roleManager, position.Value, AppRoleType.Contact);

        await CreateRole(roleManager, AppRole.Parent, AppRoleType.Parent);
        await CreateRole(roleManager, AppRole.Student, AppRoleType.Student);
        await CreateRole(roleManager, AppRole.Staff, AppRoleType.Staff);
    }

    private static async Task<AppRole?> CreateRole(RoleManager<AppRole> roleManager, string roleName, AppRoleType type)
    {
        AppRole? existing = await roleManager.FindByNameAsync(roleName);

        if (existing is null)
            await roleManager.CreateAsync(new AppRole(roleName, type));

        return null;
    }

    private static async Task CreateRoleWithPermission(RoleManager<AppRole> roleManager, string roleName, List<AuthPermission> permissions, AppRoleType type)
    {
        AppRole? role = await CreateRole(roleManager, roleName, type);
        
        IList<Claim> claims = await roleManager.GetClaimsAsync(role!);

        List<Claim> permissionClaims = claims
            .Where(claim => claim.Type == AuthClaimType.Permission)
            .ToList();

        List<string> permissionValues = permissions.Select(entry => entry.Value).ToList();

        foreach (Claim claim in permissionClaims)
        {
            if (permissionValues.Contains(claim.Value))
                continue;

            await roleManager.RemoveClaimAsync(role!, claim);
        }

        foreach (AuthPermission permission in permissions)
        {
            if (permissionClaims.All(claim => claim.Value != permission))
                await roleManager.AddClaimAsync(role!, new Claim(AuthClaimType.Permission, permission));
        }
    }
}
