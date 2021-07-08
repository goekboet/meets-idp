using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace seedData
{
    public record UserSeed
    {
        public string Email {get; init;}
        public string Password {get;init;}

        public string Name => Email
            .Substring(0, Email.IndexOf('@'))
            .Replace('.', ' ');

    } 
    
    class Program
    {
        static UserSeed FromInput(string i)
        {
            var s = i.Split(" - ");
            return new UserSeed { Email = s[1], Password = s[0] };
        }

        static async Task RecordUser(
            UserManager<ApplicationUser> record, 
            UserSeed u)
        {
            var appUser = new ApplicationUser 
            { 
                UserName = u.Email, 
                Id = Guid.NewGuid().ToString(),
                Email = u.Email
            };
            await record.CreateAsync(appUser, u.Password);
            var nameClaim = new Claim("name", u.Name);
            await record.AddClaimAsync(appUser, nameClaim);
        }
        static async Task Main(string[] args)
        {
            var collection = new ServiceCollection();
            collection.AddDbContext<ApplicationDbContext>();

            collection.AddIdentity<ApplicationUser, IdentityRole>(o =>
            {
                o.Password.RequireNonAlphanumeric = false;
                o.Password.RequireUppercase = false;
                o.Password.RequireDigit = false;
            })
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();
            collection.AddLogging(build => build.AddConsole());

            var users = from l in File.ReadAllLines("Data/users-2021-05-30T23:44:54.txt")
                select FromInput(l);

            var sp = collection.BuildServiceProvider();
            using var scope = sp.CreateScope();
            using var usrmgr = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>(); 

            foreach (var user in users)
            {
                await RecordUser(usrmgr, user);
                logger.LogInformation($"Created user: {user.Name}");
            }

            logger.LogInformation("Done");
        }
    }
}
