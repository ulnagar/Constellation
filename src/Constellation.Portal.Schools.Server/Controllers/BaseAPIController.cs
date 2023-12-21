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
            Claim userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
                return null;

            string userId = userIdClaim.Value;

            return await UserManager.FindByIdAsync(userId);
        }

        return null;
    }

    protected async Task<bool> IsUserAdmin(AppUser user)
    {
        if (User.Identity is not null && User.Identity.IsAuthenticated)
        {
            IList<string> roles = await UserManager.GetRolesAsync(user);
            if (roles.Contains(AuthRoles.Admin))
            {
                return true;
            }
        }

        return false;
    }

    protected async Task<List<string>> GetCurrentUserSchools()
    {
        List<string> schoolCodes = new List<string>();

        if (User.Identity is not null && User.Identity.IsAuthenticated)
        {
            IEnumerable<Claim> schoolCodeClaims = User.FindAll(AuthClaimType.SchoolCode);

            if (!schoolCodes.Any())
            {
                AppUser user = await GetCurrentUser();

                IList<Claim> claims = await _userManager.GetClaimsAsync(user);

                schoolCodeClaims = claims.Where(claim => claim.Type == "Schools").ToList();
            }

            if (!schoolCodeClaims.Any())
                return schoolCodes;

            foreach (Claim claim in schoolCodeClaims)
            {
                if (claim.Value.Contains(','))
                {
                    string[] codes = claim.Value.Split(',');
                    foreach (string code in codes)
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
