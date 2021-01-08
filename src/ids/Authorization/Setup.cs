using Microsoft.Extensions.DependencyInjection;

namespace Ids.Authorization
{
    public static class Setup
    {
        public static void SetupAuthorization(
            this IServiceCollection services
        )
        {
            services.AddAuthorization(opts =>
                opts.AddPolicy("OpenId", Policies.OpenId)
            );
        }
    }
}