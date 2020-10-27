using Microsoft.Extensions.DependencyInjection;

namespace Ids.Login
{
    public static class Setup
    {
        public static void SetupLogin(
            this IServiceCollection s
        )
        {
            s.AddScoped<IVerifyCredentials, AspIdentityLogin>();
        }
    }
}