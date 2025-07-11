﻿namespace Constellation.Infrastructure.Identity.ClaimsPrincipalFactories;

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
            new Claim(ClaimTypes.GivenName, user.FirstName),
            new Claim(ClaimTypes.Surname, user.LastName)
        });

        if (user.IsStaffMember)
        {
            identity.AddClaim(new Claim(AuthClaimType.StaffEmployeeId, user.StaffId.ToString()));
        }

        if (user.IsStudent)
        {
            identity.AddClaim(new Claim(AuthClaimType.StudentId, user.StudentId.ToString()));
        }

        return identity;
    }
}
