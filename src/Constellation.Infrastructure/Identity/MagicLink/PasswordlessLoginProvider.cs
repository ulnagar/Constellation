namespace Constellation.Infrastructure.Identity.MagicLink;

using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

public class PasswordlessLoginProvider<TUser> : TotpSecurityStampBasedTokenProvider<TUser> where TUser : class
{
    public override Task<bool> CanGenerateTwoFactorTokenAsync(UserManager<TUser> manager, TUser user)
    {
        return Task.FromResult(false);
    }

    public override async Task<string> GetUserModifierAsync(string purpose, UserManager<TUser> manager, TUser user)
    {
        var userId = await manager.GetUserIdAsync(user);

        return "PasswordlessLogin:" + purpose + ":" + userId;
    }
}
