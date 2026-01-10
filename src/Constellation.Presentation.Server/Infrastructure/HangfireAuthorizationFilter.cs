namespace Constellation.Presentation.Server.Infrastructure;

using Constellation.Application.Models.Auth;
using Hangfire.Dashboard;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
{
    private readonly IAuthorizationService _authorizationService;

    public HangfireAuthorizationFilter(
        IAuthorizationService authorizationService)
    {
        _authorizationService = authorizationService;
    }

    public bool Authorize(DashboardContext context)
    {
        ClaimsPrincipal user = context.GetHttpContext().User;

        AuthorizationResult isAdmin = (_authorizationService.AuthorizeAsync(user, AuthPolicies.IsSiteAdmin)).Result;

        return isAdmin.Succeeded;
    }
}