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
    private readonly UserManager<AppUser> _userManager;
    private readonly RoleManager<AppRole> _roleManager;
    private readonly IOptions<IdentityOptions> _options;

    public CustomUserPropertiesClaimsFactory(
        UserManager<AppUser> userManager, 
        RoleManager<AppRole> roleManager, 
        IOptions<IdentityOptions> options) 
        : base(userManager, roleManager, options)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _options = options;
    }

    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(AppUser user)
    {
        ClaimsIdentity identity = await base.GenerateClaimsAsync(user);

        identity.AddClaims([
            new Claim(ClaimTypes.GivenName, user.Name.FirstName),
            new Claim(ClaimTypes.Surname, user.Name.LastName)
        ]);

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
        
        // Add Role Claims to user
        IEnumerable<Claim> roleClaims = identity.FindAll(ClaimTypes.Role);

        foreach (Claim roleClaim in roleClaims)
        {
            AppRole? role = await _roleManager.FindByNameAsync(roleClaim.Value);
            if (role == null) continue;

            IList<Claim> claims = await _roleManager.GetClaimsAsync(role);

            foreach (Claim claim in claims)
            {
                if (!identity.HasClaim(claim.Type, claim.Value))
                {
                    identity.AddClaim(claim);
                }
            }
        }
        
        return identity;
    }
}
