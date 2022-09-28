namespace Constellation.Infrastructure.Identity.ProfileService;

using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using IdentityModel;
using System.Threading.Tasks;

public class ParentPortalProfileService : IProfileService
{
    public ParentPortalProfileService() { }

    public Task GetProfileDataAsync(ProfileDataRequestContext context)
    {
        var emailClaims = context.Subject.FindAll(JwtClaimTypes.Email);
        context.IssuedClaims.AddRange(emailClaims);

        var nameClaims = context.Subject.FindAll(JwtClaimTypes.Name);
        context.IssuedClaims.AddRange(nameClaims);
        return Task.CompletedTask;
    }

    public Task IsActiveAsync(IsActiveContext context)
    {
        return Task.CompletedTask;
    }
}
