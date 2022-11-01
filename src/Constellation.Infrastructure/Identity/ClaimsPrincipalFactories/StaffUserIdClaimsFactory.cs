namespace Constellation.Infrastructure.Identity.ClaimsPrincipalFactories;

using Constellation.Application.Models.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Threading.Tasks;

public class StaffUserIdClaimsFactory : UserClaimsPrincipalFactory<AppUser>
{
    public StaffUserIdClaimsFactory(UserManager<AppUser> userManager, IOptions<IdentityOptions> optionsAccessor) 
        : base(userManager, optionsAccessor)
    {
    }

    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(AppUser user)
    {
        ClaimsIdentity identity = await base.GenerateClaimsAsync(user);
        
        if (user.IsStaffMember)
        {
            identity.AddClaim(new Claim("StaffId", user.StaffId));
        }

        return identity;
    }
}
