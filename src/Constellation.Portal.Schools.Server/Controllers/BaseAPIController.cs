#nullable disable
namespace Constellation.Portal.Schools.Server.Controllers;

using Constellation.Application.Models.Auth;
using Constellation.Application.Models.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

[Authorize]
[ApiController]
public class BaseAPIController : ControllerBase
{
    private UserManager<AppUser> _userManager;
    protected UserManager<AppUser> UserManager => _userManager ??= HttpContext.RequestServices.GetService<UserManager<AppUser>>();

    protected async Task<AppUser> GetCurrentUser()
    {
        if (User.Identity is not { IsAuthenticated: true }) return null;
        
        Claim userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

        if (userIdClaim == null)
            return null;

        string userId = userIdClaim.Value;

        return await UserManager.FindByIdAsync(userId);
    }

    protected async Task<bool> IsUserAdmin(AppUser user)
    {
        if (User.Identity is null || !User.Identity.IsAuthenticated) return false;

        return await UserManager.IsInRoleAsync(user, AuthRoles.Admin);
    }

    protected async Task<List<string>> GetCurrentUserSchools()
    {
        List<string> schoolCodes = new();

        if (User.Identity is null || !User.Identity.IsAuthenticated) return schoolCodes;
        
        List<Claim> schoolCodeClaims = User.FindAll(AuthClaimType.SchoolCode).ToList();

        if (schoolCodeClaims.Count == 0) // If schoolCodes is empty
        {
            AppUser user = await GetCurrentUser();

            IList<Claim> claims = await _userManager.GetClaimsAsync(user);

            schoolCodeClaims = claims.Where(claim => claim.Type == "Schools").ToList();
        }

        if (schoolCodeClaims.Count == 0) // If schoolCodeClaims is still empty
            return schoolCodes;

        foreach (Claim claim in schoolCodeClaims)
        {
            if (claim.Value.Contains(','))
            {
                string[] codes = claim.Value.Split(',');
                schoolCodes.AddRange(codes);
            }
            else
            {
                schoolCodes.Add(claim.Value);
            }
        }

        return schoolCodes;
    }
}
