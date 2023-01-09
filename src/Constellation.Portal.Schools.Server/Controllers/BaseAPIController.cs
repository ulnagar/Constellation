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

    protected List<string> GetCurrentUserSchools()
    {
        var schoolCodes = new List<string>();

        if (User.Identity is not null && User.Identity.IsAuthenticated)
        {
            var schoolCodeClaims = User.FindAll(AuthClaimType.SchoolCode);

            if (schoolCodeClaims is null || schoolCodeClaims.Count() == 0)
                return schoolCodes;

            schoolCodes = schoolCodeClaims.Select(claim => claim.Value).ToList();
        }

        return schoolCodes;
    }
}
