using IdentityServer4;
using Microsoft.AspNetCore.Authorization;

namespace Ids.Authorization
{
    public static class Policies
    {
        public static void OpenId(this AuthorizationPolicyBuilder b)
        {
            b.AddAuthenticationSchemes(IdentityServerConstants.LocalApi.AuthenticationScheme);
            b.RequireAuthenticatedUser();
        }
    }
}