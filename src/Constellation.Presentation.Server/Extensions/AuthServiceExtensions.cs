namespace Constellation.Presentation.Server.Extensions;

using Constellation.Application.Models.Identity;
using Constellation.Core.Models.Auth;
using Constellation.Infrastructure.Identity.Authorization;
using Constellation.Infrastructure.Identity.ClaimsPrincipalFactories;
using Constellation.Infrastructure.Identity.MagicLink;
using Constellation.Infrastructure.Persistence.ConstellationContext;
using Helpers.Identity;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

public static class AuthServiceExtensions
{
    public static IServiceCollection AddConstellationAuth(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddIdentity<AppUser, AppRole>()
            .AddClaimsPrincipalFactory<CustomUserPropertiesClaimsFactory>()
            .AddEntityFrameworkStores<ConstellationDbContext>()
            .AddDefaultTokenProviders()
            .AddPasswordlessLoginProvider();

        services.Configure<IdentityOptions>(options =>
        {
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireUppercase = true;
            options.Password.RequiredLength = 6;
            options.Password.RequiredUniqueChars = 1;
            options.User.RequireUniqueEmail = true;
        });

        services
            .AddAuthorization(opt => opt.AddApplicationPolicies())
            .AddAuthorizationPolicies();

        services.ConfigureApplicationCookie(options =>
        {
            options.Cookie.Name = "Constellation.Identity";
            options.ExpireTimeSpan = TimeSpan.FromMinutes(90);
            options.SlidingExpiration = true;
            options.LoginPath = new PathString("/Auth/Login");
            options.LogoutPath = new PathString("/Auth/Logout");
        });

        services
            .AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            })
            .AddCookie(options =>
            {
                options.Cookie.Name = "Constellation.Identity";
                options.ExpireTimeSpan = TimeSpan.FromMinutes(90);
                options.SlidingExpiration = true;
                options.LoginPath = new PathString("/Auth/Login");
                options.LogoutPath = new PathString("/Auth/Logout");
            })
            .AddOpenIdConnect(options =>
            {
#if DEBUG
                options.RequireHttpsMetadata = false;
#endif
                var oidcConfig = configuration.GetSection("OpenIDConnectSettings");

                options.Authority = oidcConfig["Authority"];
                options.ClientId = oidcConfig["ClientId"];
                options.ClientSecret = oidcConfig["ClientSecret"];
                options.CallbackPath = "/signin-oidc";
                options.ResponseType = OpenIdConnectResponseType.Code;
                options.SaveTokens = true;
                options.GetClaimsFromUserInfoEndpoint = true;
                options.Events = new OpenIdConnectEvents
                {
                    OnTokenValidated = async context =>
                    {
                        await IdentityHelpers.SyncUserWithIdentity(context);
                        IdentityHelpers.UpdateKnownUserSession(context);
                    },
                    OnRedirectToIdentityProvider = context =>
                    {
                        // Pull the hint from wherever it's available to you at
                        // challenge time — query string, TempData, a known claim, etc.
                        var loginHint = context.Properties.Items.TryGetValue("login_hint", out var hint)
                            ? hint
                            : null;

                        if (!string.IsNullOrWhiteSpace(loginHint))
                        {
                            context.ProtocolMessage.LoginHint = loginHint;
                        }

                        return Task.CompletedTask;
                    }
                };
            });

        return services;
    }
}