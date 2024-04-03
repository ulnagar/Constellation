﻿namespace Constellation.Presentation.Server.Services;

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

    private ClaimsPrincipal User => _httpContextAccessor.HttpContext?.User;

    public string UserName =>
        User is null ? string.Empty :
        User.Identity is null ? string.Empty :
        User.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.GivenName) is null ? User.Identity.Name :
        User.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.Surname) is null ? User.Identity.Name :
        $"{User.Claims.First(claim => claim.Type == ClaimTypes.GivenName).Value} {User.Claims.First(claim => claim.Type == ClaimTypes.Surname).Value}";

    public string EmailAddress =>
        User is null ? string.Empty :
        User.Identity is null ? string.Empty :
        User.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.Email) is null ? string.Empty :
        User.Claims.First(claim => claim.Type == ClaimTypes.Email).Value;

    public bool IsAuthenticated => User is not null && (User.Identity?.IsAuthenticated ?? false);
}
