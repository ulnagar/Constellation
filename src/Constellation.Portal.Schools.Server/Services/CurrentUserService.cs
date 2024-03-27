namespace Constellation.Portal.Schools.Server.Services;

using Core.Abstractions.Services;
using System.Security.Claims;

/// <summary>
/// Implementation from https://stackoverflow.com/a/63118188
/// Previous versions would provide only null replies
/// </summary>
public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

    public string? UserName => User is not null && User.Identity is not null && User.Identity.IsAuthenticated ? User.Identity.Name : string.Empty;

    public string EmailAddress => User?.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.Email)?.Value ?? string.Empty;

    public bool IsAuthenticated => User is not null && User.Identity is not null && User.Identity.IsAuthenticated;
}
