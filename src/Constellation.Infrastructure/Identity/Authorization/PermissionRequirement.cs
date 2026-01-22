namespace Constellation.Infrastructure.Identity.Authorization;

using Application.Models.Auth;
using Microsoft.AspNetCore.Authorization;

public sealed class PermissionRequirement(IEnumerable<AuthPermission> permissions) : IAuthorizationRequirement
{
    public IEnumerable<AuthPermission> Permissions { get; } = permissions;
}

public sealed class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        IEnumerable<string> userPermissions = context.User.Claims
            .Where(c => c.Type == AuthClaimType.Permission)
            .Select(c => c.Value);

        if (requirement.Permissions.Any(permission => userPermissions.Contains(permission)))
            context.Succeed(requirement);

        return Task.CompletedTask;
    }
}