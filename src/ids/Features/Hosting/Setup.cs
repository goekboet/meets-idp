using System.IO;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Ids.Hosting
{
    public static class Setup
    {
        public static void SetupProxying(
            this IServiceCollection services
        )
        {
            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.XForwardedFor |
                    ForwardedHeaders.XForwardedProto;

                // Per default kestrel only forwards proxy-headers from localhost. You need to add
                // ip-numbers into these lists or empty them to forward all.
                options.KnownNetworks.Clear();
                options.KnownProxies.Clear();
            });
        }

        public static void SetupDataProtection(
            this IServiceCollection services,
            IConfiguration config
        )
        {
            if (config["Dataprotection:Type"] == "Docker")
            {
                services.AddDataProtection()
                    .PersistKeysToFileSystem(
                        new DirectoryInfo(config["Dataprotection:KeyPath"])
                    )
                    .ProtectKeysWithCertificate(
                        new X509Certificate2(
                            config["Dataprotection:CertPath"],
                            config["Dataprotection:CertPass"]
                        )
                    );
            }
        }
    }
}