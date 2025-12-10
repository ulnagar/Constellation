namespace Constellation.Infrastructure.Identity.Authorization;

using Constellation.Application.Models.Auth;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;

public sealed class CanUseEmergencyConsoleRequirement : IAuthorizationRequirement
{
}

public sealed class HasRequiredEmergencyConsolePermissions : AuthorizationHandler<CanUseEmergencyConsoleRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, CanUseEmergencyConsoleRequirement requirement)
    {
        if (context.User.HasClaim(claim => claim is { Type: AuthClaimType.Permission, Value: AuthPermissions.EmergencyConsole.Manage }))
            context.Succeed(requirement);

        return Task.CompletedTask;
    }
}

public sealed class IsSiteAdmin : AuthorizationHandler<CanUseEmergencyConsoleRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, CanUseEmergencyConsoleRequirement requirement)
    {
        if (context.User.IsInRole(AuthRoles.Admin))
            context.Succeed(requirement);

        return Task.CompletedTask;
    }
}
