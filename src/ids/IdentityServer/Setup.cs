using Ids.AspIdentity;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Ids.Identityserver4
{
    public static class Setup
    {
        public static void ConfigureIdentityServer4(
            this IServiceCollection services,
            IConfiguration config 
        )
        {
            var builder = services.AddIdentityServer(options =>
            {
                config.GetSection("IdentityServerOptions").Bind(options);
                options.UserInteraction.LoginUrl = "/Login";
                options.UserInteraction.LogoutUrl = "/Logout";
            })
                .AddAspNetIdentity<IdsUser>()
                .AddResourceOwnerValidator<MapUsernameToEmail>()
                .AddInMemoryIdentityResources(Config.GetIdentityResources())
                .AddInMemoryApiResources(config.GetSection("Apis"))
                .AddInMemoryApiScopes(config.GetSection("ApiScopes"))
                .AddInMemoryClients(config.GetSection("Clients"))
                .AddOperationalStore(options => {
                    options.ConfigureDbContext = builder =>
                    {
                        builder.UseSqlite(
                            config.GetConnectionString("Grants"),
                            b => b.MigrationsAssembly("ids"));
                    };
                    options.EnableTokenCleanup = true;
                })
                .Services.ConfigureApplicationCookie(opts => {
                    opts.LoginPath = "/Login";
                    opts.LogoutPath = "/Logout";
                    opts.Cookie.Name = "sso";
                    opts.Cookie.IsEssential = true;
                    opts.Cookie.SameSite = SameSiteMode.None;
                    opts.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
                });
        }
    }
}