namespace Constellation.Presentation.Portal.Schools.Services;

using Constellation.Application.Interfaces.Services;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

internal sealed class CurrentUserService : ICurrentUserService
{
    private IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal User => _httpContextAccessor.HttpContext?.User;

    public string UserName => User != null && User.Identity.IsAuthenticated ? User.Identity.Name : string.Empty;

    public bool IsAuthenticated => User != null && User.Identity.IsAuthenticated;
}
