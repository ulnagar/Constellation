namespace Constellation.Infrastructure.Identity.ClaimsPrincipalFactories;

using Constellation.Application.Models.Auth;
using Constellation.Application.Models.Identity;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Threading.Tasks;

public class StaffUserIdClaimsFactory : UserClaimsPrincipalFactory<AppUser, AppRole>
{
    public StaffUserIdClaimsFactory(UserManager<AppUser> userManager, RoleManager<AppRole> roleManager, IOptions<IdentityOptions> options) 
        : base(userManager, roleManager, options)
    {
    }

    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(AppUser user)
    {
        ClaimsIdentity identity = await base.GenerateClaimsAsync(user);
        
        if (user.IsStaffMember)
        {
            identity.AddClaim(new Claim(AuthClaimType.StaffEmployeeId, user.StaffId));
        }

        return identity;
    }
}

public sealed class StaffUserIdProfileService : IProfileService
{
    private readonly UserManager<AppUser> _userManager;

    public StaffUserIdProfileService(
        UserManager<AppUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task GetProfileDataAsync(ProfileDataRequestContext context)
    {
        string username = context.Subject.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(username))
            return;

        AppUser user = await _userManager.FindByNameAsync(username);

        if (user is null)
            return;

        if (user.IsStaffMember)
            context.IssuedClaims.Add(new(AuthClaimType.StaffEmployeeId, user.StaffId));
    }

    public Task IsActiveAsync(IsActiveContext context) => Task.CompletedTask;
}