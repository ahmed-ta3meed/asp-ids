using Duende.IdentityServer.Models;

namespace duende;

public static class Config
{
    public static IEnumerable<IdentityResource> IdentityResources =>
        new IdentityResource[]
        {
            new IdentityResources.OpenId(),
            new IdentityResources.Profile(),
        };

    public static IEnumerable<ApiScope> ApiScopes =>
        new ApiScope[]
        {
            new ApiScope("scope1"),
            new ApiScope("scope2"),
        };
    public static IEnumerable<Client> Clients =>
        new Client[]
        {
            // m2m client credentials flow client
            new Client
            {
                ClientId = "m2m.client",
                ClientName = "Client Credentials Client",

                AllowedGrantTypes = {
                    //GrantTypes.Ciba.First(),
                    //GrantTypes.ClientCredentials.First(),
                    //GrantTypes.Code.First(),
                    //GrantTypes.CodeAndClientCredentials.First(),
                    //GrantTypes.DeviceFlow.First(),
                    //GrantTypes.Hybrid.First(),
                    //GrantTypes.HybridAndClientCredentials.First(),
                    //GrantTypes.Implicit.First(),
                    //GrantTypes.ImplicitAndClientCredentials.First(),
                    //GrantTypes.ResourceOwnerPassword.First(),
                    //GrantTypes.ResourceOwnerPasswordAndClientCredentials.First(),
                    GrantTypes.ResourceOwnerPassword.First()
                },
                ClientSecrets = { new Secret("secret".Sha256()) },
                AllowOfflineAccess= true,
                AllowedScopes = { "scope1", "openid", "profile" },
                RedirectUris = { "https://localhost:44300/signin-oidc" },
            },

            // interactive client using code flow + pkce
            new Client
            {
                ClientId = "interactive",
                ClientSecrets = { new Secret("49C1A7E1-0C79-4A89-A3D6-A37998FB86B0".Sha256()) },

                AllowedGrantTypes = GrantTypes.Code,

                RedirectUris = { "https://localhost:44300/signin-oidc" }, 
                FrontChannelLogoutUri = "https://localhost:44300/signout-oidc",
                PostLogoutRedirectUris = { "https://localhost:44300/signout-callback-oidc" },

                AllowOfflineAccess = true,
                AllowedScopes = { "openid", "profile", "scope2" }
            },
        };
}
