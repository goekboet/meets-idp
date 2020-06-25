using IdentityServer4.Stores;
using Microsoft.Extensions.DependencyInjection;

namespace gateway.Pki
{
    public static class Startup
    {
        public static void SetupKeyStore(
            this IServiceCollection services
        )
        {
            services.AddTransient<ISigningCredentialStore, OnFileSigningKeyStore>();
            services.AddTransient<IValidationKeysStore, OnFileKeysetStore>();
        }
    }
}