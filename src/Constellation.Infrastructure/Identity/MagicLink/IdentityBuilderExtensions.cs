namespace Constellation.Infrastructure.Identity.MagicLink;

using Microsoft.AspNetCore.Identity;

public static class IdentityBuilderExtensions
{
    public static IdentityBuilder AddPasswordlessLoginProvider(this IdentityBuilder builder)
    {
        var userType = builder.UserType;
        var totpProvider = typeof(PasswordlessLoginProvider<>).MakeGenericType(userType);
        return builder.AddTokenProvider("PasswordlessLoginProvider", totpProvider);
    }
}
