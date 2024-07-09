namespace Constellation.Infrastructure.Identity.MagicLink;

using Microsoft.AspNetCore.Identity;

public static class IdentityBuilderExtensions
{
    public static IdentityBuilder AddPasswordlessLoginProvider(this IdentityBuilder builder)
    {
        Type userType = builder.UserType;
        Type totpProvider = typeof(PasswordlessLoginProvider<>).MakeGenericType(userType);
        return builder.AddTokenProvider("PasswordlessLoginProvider", totpProvider);
    }
}
