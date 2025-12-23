namespace Constellation.Infrastructure.Identity.Authorization;

using Application.Models.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

public sealed class PermissionAuthorizationPolicyProvider : DefaultAuthorizationPolicyProvider
{
    public PermissionAuthorizationPolicyProvider(IOptions<AuthorizationOptions> options) 
        : base(options) { }

    public override async Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        AuthorizationPolicy? policy = await base.GetPolicyAsync(policyName);

        if (policy is not null)
            return policy;

        string[] permissionNames = policyName.Split(',');

        List<AuthPermission> permissions = [];

        foreach (var permissionName in permissionNames)
        {
            AuthPermission? permission = AuthPermission.FromValue(permissionName);

            if (permission is not null)
                permissions.Add(permission);
        }

        return new AuthorizationPolicyBuilder()
            .AddRequirements(new PermissionRequirement(permissions))
            .Build();
    }
}