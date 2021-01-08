using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Ids.AspIdentity
{
    public static class Setup
    {
        public static void ConfigureAspIdentity(
            this IServiceCollection services,
            IConfiguration configuration
        )
        {
            services.AddDbContext<UsersDb>(options =>
                options.UseNpgsql(
                            configuration.GetConnectionString("Users"),
                            b => b.MigrationsAssembly("ids")));

            services.AddIdentity<IdsUser, IdentityRole>(options =>
                {
                    options.Password.RequiredLength = 8;
                    options.Password.RequireDigit = false;
                    options.Password.RequireNonAlphanumeric = false;
                    options.Password.RequireUppercase = false;
                    options.User.RequireUniqueEmail = true;
                })
                .AddEntityFrameworkStores<UsersDb>()
                .AddDefaultTokenProviders();

            services.AddAuthentication().AddLocalApi();
        }
    }
}