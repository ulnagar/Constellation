namespace Constellation.Presentation.Server.Services;

using Application.Models.Auth;
using Core.Abstractions.Services;
using Core.Models.StaffMembers.Identifiers;
using Microsoft.AspNetCore.Authentication;
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

    /// <summary>
    /// Returns the Guid Id for an EnrolmentUser if they are currently logged in. Returns null for all other instances.
    /// </summary>
    /// <returns>Nullable Guid</returns>
    public async Task<Guid?> GetUserId()
    {
        HttpContext? ctx = _httpContextAccessor.HttpContext;

        if (ctx is null)
            return null;

        AuthenticateResult portalResult = await ctx.AuthenticateAsync("EnrolmentsPortal");
        if (portalResult.Succeeded)
        {
            string? userIdString = portalResult.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userIdString))
                return null;

            bool guidConversion = Guid.TryParse(userIdString, out var userId);

            if (guidConversion)
                return userId;
        }

        return null;
    }

    public string UserName => Identify();

    public string EmailAddress =>
        User is null ? string.Empty :
        User.Identity is null ? string.Empty :
        User.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.Email) is null ? string.Empty :
        User.Claims.First(claim => claim.Type == ClaimTypes.Email).Value;

    public bool IsAuthenticated => User is not null && (User.Identity?.IsAuthenticated ?? false);

    public StaffId StaffId => GetStaffId();

    private StaffId GetStaffId()
    {
        string? claimStaffId = User?.Claims.FirstOrDefault(claim => claim.Type == AuthClaimType.StaffEmployeeId)?.Value;

        if (claimStaffId is null)
            return StaffId.Empty;

        bool guidSuccess = Guid.TryParse(claimStaffId, out Guid guidStaffId);

        if (!guidSuccess)
            return StaffId.Empty;

        return StaffId.FromValue(guidStaffId);
    }

    private string Identify()
    {
        HttpContext? ctx = _httpContextAccessor.HttpContext;

        if (ctx is null)
            return "System";

        if (User is not null
            && User.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.GivenName) is not null
            && User.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.Surname) is not null)
            return $"{User.Claims.First(claim => claim.Type == ClaimTypes.GivenName).Value} {User.Claims.First(claim => claim.Type == ClaimTypes.Surname).Value}";
        
        if (ctx.Items.TryGetValue("OfferToken", out var token) && token is string offerToken)
            return $"Anonymous via offer token {offerToken}";
        
        return "Anonymous";
    }
}
