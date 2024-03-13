namespace Constellation.Presentation.Server.Identity;

using Duende.IdentityServer;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Test;

internal static class IdentityConfiguration
{
    public static IEnumerable<ApiScope> ApiScopes =>
        new List<ApiScope>
        {
            new("SCOPE.ACOS.Schools", "Schools Portal Scope"),
            new("SCOPE.ACOS.Parents", "Parents Portal Scope")
        };

    public static IEnumerable<Client> Clients =>
        new List<Client>
        {
            new()
            {
                ClientId = "CLIENT-ACOS-Staff",
                ClientSecrets = { new Secret("TESTSECRET".Sha256()) },
                AllowedGrantTypes = GrantTypes.Implicit,
                AllowedScopes =
                {
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile,
                    IdentityServerConstants.StandardScopes.Email
                },

                RedirectUris = { "https://localhost:44350/signin-oidc" },
                PostLogoutRedirectUris = { "https://localhost:44350/signout-callback-oidc" }
            },
            new()
            {
                ClientId = "schools",
                ClientSecrets = { new Secret("TESTSECRET".Sha256()) },
                AllowedGrantTypes = GrantTypes.ImplicitAndClientCredentials,
                AllowedScopes =
                {
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile,
                    IdentityServerConstants.StandardScopes.Email
                },

                RedirectUris = { "https://localhost:44350/signin-oidc" },
                PostLogoutRedirectUris = { "https://localhost:44350/signout-callback-oidc" }
            },
            new()
            {
                ClientId = "CLIENT-ACOS-Parents",
                ClientSecrets = { new Secret("TESTSECRET".Sha256()) },
                AllowedGrantTypes = GrantTypes.ClientCredentials,
                AllowedScopes = { "SCOPE.ACOS.Parents" }
            }
        };

    public static List<TestUser> Users =>
        new()
        {
            new()
            {
                SubjectId = "1",
                Username = "user1",
                Password = "password"
            },
            new()
            {
                SubjectId = "2",
                Username = "user2",
                Password = "password"
            }
        };
}
