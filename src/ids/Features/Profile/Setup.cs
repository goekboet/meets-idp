using Microsoft.Extensions.DependencyInjection;

namespace Ids.Profile
{
    public static class Setup
    {
        public static void SetupProfile(
            this IServiceCollection services
        )
        {
            services.AddScoped<IGetProfile, AspIdentityGetProfile>();
            services.AddScoped<ISetPassword, AspIdentitySetPassword>();
            services.AddScoped<ISetName, AspIdentitySetName>();
            services.AddScoped<IChangePassword, AspIdentityChangePassword>();
        }
    }
}