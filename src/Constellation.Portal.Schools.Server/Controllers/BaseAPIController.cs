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
        if (User.Identity != null && User.Identity.IsAuthenticated)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
                return null;

            var userId = userIdClaim.Value;

            return await UserManager.FindByIdAsync(userId);
        }

        return null;
    }

    protected async Task<bool> IsUserAdmin(AppUser user)
    {
        if (User.Identity is not null && User.Identity.IsAuthenticated)
        {
            var roles = await UserManager.GetRolesAsync(user);
            if (roles.Contains(AuthRoles.Admin))
            {
                return true;
            }
        }

        return false;
    }

    protected async Task<List<string>> GetCurrentUserSchools()
    {
        var schoolCodes = new List<string>();

        if (User.Identity is not null && User.Identity.IsAuthenticated)
        {
            var schoolCodeClaims = User.FindAll(AuthClaimType.SchoolCode);

            if (schoolCodes is null || !schoolCodes.Any())
            {
                var user = await GetCurrentUser();

                var claims = await _userManager.GetClaimsAsync(user);

                schoolCodeClaims = claims.Where(claim => claim.Type == "Schools").ToList();
            }

            if (schoolCodeClaims is null || !schoolCodeClaims.Any())
                return schoolCodes;

            foreach (var claim in schoolCodeClaims)
            {
                if (claim.Value.Contains(','))
                {
                    var codes = claim.Value.Split(',');
                    foreach (var code in codes)
                    {
                        schoolCodes.Add(code);
                    }
                }
                else
                {
                    schoolCodes.Add(claim.Value);
                }
            }
        }

        return schoolCodes;
    }
}
