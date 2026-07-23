namespace Constellation.Infrastructure.Identity.Authorization;

using Application.Models.Auth;
using Application.Models.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Claims;

public sealed class PermissionRequirement(IEnumerable<AuthPermission> permissions) : IAuthorizationRequirement
{
    public IEnumerable<AuthPermission> Permissions { get; } = permissions;
}

public sealed class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly RoleManager<AppRole> _roleManager;
    private readonly IMemoryCache _cache;

    public PermissionAuthorizationHandler(
        RoleManager<AppRole> roleManager, 
        [FromKeyedServices("AuthPermissions")] IMemoryCache cache)
    {
        _roleManager = roleManager;
        _cache = cache;
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        IEnumerable<string> roleNames = context.User.Claims
            .Where(c => c.Type == ClaimTypes.Role)
            .Select(c => c.Value);

        foreach (string roleName in roleNames)
        {
            IEnumerable<string> rolePermissions = await GetCachedRolePermissions(roleName);

            if (requirement.Permissions.Any(permission => rolePermissions.Contains(permission.Value)))
            {
                context.Succeed(requirement);
                return;
            }
        }
    }

    private async Task<IEnumerable<string>> GetCachedRolePermissions(string roleName)
    {
        return await _cache.GetOrCreateAsync($"role-permissions:{roleName}", async entry =>
        {
            entry.SlidingExpiration = TimeSpan.FromMinutes(15);
            entry.Size = 1;

            AppRole? role = await _roleManager.FindByNameAsync(roleName);
            if (role is null)
                return Enumerable.Empty<string>();

            IList<Claim> claims = await _roleManager.GetClaimsAsync(role);

            return claims
                .Where(c => c.Type == AuthClaimType.Permission)
                .Select(c => c.Value)
                .ToList();
        }) ?? Enumerable.Empty<string>();
    }
}