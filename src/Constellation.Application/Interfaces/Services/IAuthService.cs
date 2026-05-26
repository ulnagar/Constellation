namespace Constellation.Application.Interfaces.Services;

using System.Security.Claims;

public interface IAuthService
{
    Task<ImpersonationResult> ImpersonateAsync(Guid targetUserId, ClaimsPrincipal currentPrincipal);
    Task<ImpersonationResult> StopImpersonatingAsync(ClaimsPrincipal currentPrincipal);
    bool IsImpersonating(ClaimsPrincipal principal);
}

public record ImpersonationResult(bool Succeeded, string? ErrorMessage = null);

