namespace Constellation.Infrastructure.DependencyInjection;

using Constellation.Application.Models.Auth;
using Constellation.Application.Models.Identity;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

public static class IdentityDefaults
{
    public static async Task SeedRoles(RoleManager<AppRole> roleManager)
    {
        List<AuthPermission> permissions = AuthPermission.GetOptions.ToList();

        await CreateRoleWithPermission(roleManager, "SuperAdmin", permissions);
    }

    private static async Task CreateRoleWithPermission(RoleManager<AppRole> roleManager, string roleName, List<AuthPermission> permissions)
    {
        AppRole? existing = await roleManager.FindByNameAsync(roleName);

        if (existing is null)
            await roleManager.CreateAsync(new AppRole { Name = roleName });

        AppRole? role = existing ?? await roleManager.FindByNameAsync(roleName);
        
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
