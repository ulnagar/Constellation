namespace Constellation.Infrastructure.Services;

using Application.Models.Auth;
using Application.Models.Identity;
using Constellation.Application.Interfaces.Services;
using Constellation.Core.Models.Auth;
using Core.Models.Auth.Enums;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Claims;

internal class AuthService : IAuthService
{
    private readonly UserManager<AppUser> _userManager;
    private readonly RoleManager<AppRole> _roleManager;
    private readonly SignInManager<AppUser> _signInManager;
    private readonly IMemoryCache _cache;
    private readonly ILogger _logger;

    private static readonly TimeSpan RoleClaimsCacheDuration = TimeSpan.FromMinutes(15);

    public AuthService(
        UserManager<AppUser> userManager,
        RoleManager<AppRole> roleManager,
        SignInManager<AppUser> signInManager,
        [FromKeyedServices("AuthService")] IMemoryCache cache,
        ILogger logger)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _signInManager = signInManager;
        _cache = cache;
        _logger = logger
            .ForContext<IAuthService>();
    }

    public async Task<ImpersonationResult> ImpersonateAsync(
        Guid targetUserId,
        ClaimsPrincipal currentPrincipal)
    {
        // Prevent nested impersonation
        if (IsImpersonating(currentPrincipal))
            return new(false, "Cannot impersonate while already impersonating.");

        var originalUser = await _userManager.GetUserAsync(currentPrincipal);
        if (originalUser is null)
            return new(false, "Could not resolve the current user.");

        var targetUser = await _userManager.FindByIdAsync(targetUserId.ToString());
        if (targetUser is null)
            return new(false, "Target user not found.");

        // Build a ClaimsPrincipal for the target user...
        var targetPrincipal = await _signInManager.CreateUserPrincipalAsync(targetUser);

        // ...then inject impersonation metadata into it
        var identity = (ClaimsIdentity)targetPrincipal.Identity!;
        identity.AddClaims([
            new Claim(AuthClaimType.IsImpersonating,  "true"),
            new Claim(AuthClaimType.OriginalUserId, originalUser.Id.ToString()),
            new Claim(AuthClaimType.OriginalUserName, originalUser.UserName!),
        ]);

        await _signInManager.SignOutAsync();
        await _signInManager.Context.SignInAsync(
            IdentityConstants.ApplicationScheme,
            targetPrincipal);

        _logger.Warning(
            "Impersonation started: admin {AdminId} is impersonating {TargetId}",
            originalUser.Id, targetUser.Id);

        return new(true);
    }

    public async Task<ImpersonationResult> StopImpersonatingAsync(
        ClaimsPrincipal currentPrincipal)
    {
        if (!IsImpersonating(currentPrincipal))
            return new(false, "Not currently impersonating.");

        string? originalUserIdString = currentPrincipal.FindFirstValue(AuthClaimType.OriginalUserId);

        if (!Guid.TryParse(originalUserIdString, out Guid originalUserId))
            return new(false, "Original user ID claim was invalid.");

        AppUser? originalUser = await _userManager.FindByIdAsync(originalUserId.ToString());

        if (originalUser is null)
            return new(false, "Original admin account not found.");

        // Sign back in as the original admin — full role/claims refresh
        await _signInManager.SignOutAsync();
        await _signInManager.SignInAsync(originalUser, isPersistent: false);

        _logger.Warning("Impersonation ended: admin {AdminId} restored", originalUser.Id);

        return new(true);
    }

    public bool IsImpersonating(ClaimsPrincipal principal) =>
        principal.FindFirstValue(AuthClaimType.IsImpersonating) == "true";

    public async Task<bool> UserHasPermission(AppUser user, AuthPermission permission, CancellationToken cancellationToken = default)
    {
        IList<Claim> userClaims = await _userManager.GetClaimsAsync(user);
        if (userClaims.Any(c => c.Type == AuthClaimType.Permission && c.Value == permission.Value))
            return true;

        // 2. Check claims on each role the user belongs to
        IList<string> roleNames = await _userManager.GetRolesAsync(user);

        foreach (string roleName in roleNames)
        {
            IReadOnlyList<Claim> roleClaims = await GetCachedRoleClaims(roleName);

            if (roleClaims.Any(c => c.Type == AuthClaimType.Permission && c.Value == permission.Value))
                return true;
        }

        return false;
    }

    private async Task<IReadOnlyList<Claim>> GetCachedRoleClaims(string roleName)
    {
        string cacheKey = $"role-claims:{roleName}";

        if (_cache.TryGetValue(cacheKey, out IReadOnlyList<Claim>? cached) && cached is not null)
            return cached;

        AppRole? role = await _roleManager.FindByNameAsync(roleName);

        IReadOnlyList<Claim> claims = role is null
            ? []
            : (await _roleManager.GetClaimsAsync(role)).ToList();

        _cache.Set(cacheKey, claims, RoleClaimsCacheDuration);

        return claims;
    }

    public void InvalidateRoleClaimsCache(string roleName) =>
        _cache.Remove($"role-claims:{roleName}");

}