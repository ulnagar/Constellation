namespace Constellation.Infrastructure.Identity.ProfileService;

using Constellation.Application.Models.Identity;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using IdentityModel;
using Microsoft.AspNetCore.Identity;
using System.Linq;
using System.Threading.Tasks;

public class WASMAuthenticationProfileService : IProfileService
{
    private readonly UserManager<AppUser> _userManager;

    public WASMAuthenticationProfileService(UserManager<AppUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task GetProfileDataAsync(ProfileDataRequestContext context)
    {
        var emailClaims = context.Subject.FindAll(JwtClaimTypes.Email);
        context.IssuedClaims.AddRange(emailClaims);

        // Override the client side token claims with the user DisplayName property instead of email
        AppUser user;

        if (context.Subject.Identity.Name.Contains('@'))
        {
            user = await _userManager.FindByEmailAsync(context.Subject.Identity.Name);
        }
        else
        {
            user = await _userManager.FindByEmailAsync(emailClaims.First().Value);
        }

        context.IssuedClaims.Add(new System.Security.Claims.Claim(JwtClaimTypes.Name, user.DisplayName));
    }

    public Task IsActiveAsync(IsActiveContext context)
    {
        return Task.CompletedTask;
    }
}
