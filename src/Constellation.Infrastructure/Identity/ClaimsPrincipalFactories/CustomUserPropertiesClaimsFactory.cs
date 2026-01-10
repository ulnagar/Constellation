namespace Constellation.Infrastructure.Identity.ClaimsPrincipalFactories;

using Application.Models.Identity.Enums;
using Constellation.Application.Models.Auth;
using Constellation.Application.Models.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Threading.Tasks;

public class CustomUserPropertiesClaimsFactory : UserClaimsPrincipalFactory<AppUser, AppRole>
{
    public CustomUserPropertiesClaimsFactory(
        UserManager<AppUser> userManager, 
        RoleManager<AppRole> roleManager, 
        IOptions<IdentityOptions> options) 
        : base(userManager, roleManager, options)
    {
    }

    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(AppUser user)
    {
        ClaimsIdentity identity = await base.GenerateClaimsAsync(user);

        identity.AddClaims(new []
        {
            new Claim(ClaimTypes.GivenName, user.Name.FirstName),
            new Claim(ClaimTypes.Surname, user.Name.LastName)
        });

        if (user.IsStaffMember)
        {
            AppUserLink? link = user.Links.FirstOrDefault(link => !link.IsDeleted && link.Type == LinkType.Staff);

            if (link is not null)
                identity.AddClaim(new Claim(AuthClaimType.StaffEmployeeId, link.LinkId.ToString()));
        }

        if (user.IsStudent)
        {
            AppUserLink? link = user.Links.FirstOrDefault(link => !link.IsDeleted && link.Type == LinkType.Student);

            if (link is not null)
                identity.AddClaim(new Claim(AuthClaimType.StudentId, link.LinkId.ToString()));
        }

        return identity;
    }
}
