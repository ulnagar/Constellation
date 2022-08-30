using Constellation.Application.Models.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Constellation.Infrastructure.Identity.MagicLink;

public class DisplayNameClaimTransformation : IClaimsTransformation
{
    private readonly UserManager<AppUser> _userManager;

    public DisplayNameClaimTransformation(UserManager<AppUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        if (principal.Identity.Name == null)
            return principal;

        ClaimsIdentity claimsIdentity = new();

        var claimType = "DisplayName";

        var user = await _userManager.FindByEmailAsync(principal.Identity.Name);
        var claimValue = $"{user.FirstName} {user.LastName}";

        if (!principal.HasClaim(claim => claim.Type == claimType))
        {
            claimsIdentity.AddClaim(new Claim(claimType, claimValue));
        }

        principal.AddIdentity(claimsIdentity);
        return principal;
    }
}
