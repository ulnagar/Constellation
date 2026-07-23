namespace Constellation.Application.Interfaces.Services;

using Core.Models.Auth;
using Models.Auth;
using System.Security.Claims;

public interface IAuthService
{
    Task<ImpersonationResult> ImpersonateAsync(Guid targetUserId, ClaimsPrincipal currentPrincipal);
    Task<ImpersonationResult> StopImpersonatingAsync(ClaimsPrincipal currentPrincipal);
    bool IsImpersonating(ClaimsPrincipal principal);

    Task<bool> UserHasPermission(AppUser user, AuthPermission permission, CancellationToken cancellationToken = default);
    void InvalidateRoleClaimsCache(string roleName);
}

public record ImpersonationResult(bool Succeeded, string? ErrorMessage = null);