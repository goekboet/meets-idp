using IdentityServer4.Stores;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace gateway.Pki
{
    public static class Startup
    {
        public static void SetupKeyStoreMigration(
            this IServiceCollection services,
            IConfiguration config
        )
        {
            var plan = new KeyStoreMigrationPlan();
            config.GetSection("Featureflags").Bind(plan);

            if (plan.RetireLegacyKey)
            {
                services.Configure<LegacyKey>(
                    config.GetSection("Keymaterial"));
                services.AddTransient<ISigningCredentialStore, FullyMigratedSigningKeyStore>();
                services.AddTransient<IValidationKeysStore, AnnounceLegacyKeyWithMigratedKeystore>();
            }
            else if (plan.AnnounceKey1)
            {
                services.Configure<LegacyKey>(
                    config.GetSection("Keymaterial"));
                services.AddTransient<ISigningCredentialStore, LegacyKeySigningKeyStore>();
                services.AddTransient<IValidationKeysStore, AnnounceLegacyKeyWithMigratedKeystore>();
            }
            else
            {
                services.SetupKeyStore();
            }
        }

        public static void SetupKeyStore(
            this IServiceCollection services
        )
        {
            services.AddTransient<ISigningCredentialStore, FullyMigratedSigningKeyStore>();
            services.AddTransient<IValidationKeysStore, FullyMigratedKeysetStore>();
        }
    }
}