using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Ids.AspIdentity
{
    public class IdsUser : IdentityUser { }
    
    public class UsersDb : IdentityDbContext<IdsUser>
    {
        public UsersDb(DbContextOptions<UsersDb> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            
            base.OnModelCreating(builder);

            // Customize the ASP.NET Identity model and override the defaults if needed.
            // For example, you can rename the ASP.NET Identity table names and more.
            // Add your customizations after calling base.OnModelCreating(builder);

            // var userStore = new UserStore<ApplicationUser>(new ApplicationDbContext());
            // var manager = new ApplicationUserManager(userStore);
            // var result = await manager.Create(user, password);

            string sc(string s)
            {
                if (string.IsNullOrEmpty(s)) { return s; }
                var leadingLD = Regex.Match(s, @"^_+");
                return leadingLD + Regex.Replace(s, @"([a-z0-9])([A-Z])", "$1_$2").ToLower();
            }

            foreach (var entity in builder.Model.GetEntityTypes())
            {
                // Replace table names
                entity.SetTableName(sc(entity.GetTableName()));

                // Replace column names            
                foreach (var property in entity.GetProperties())
                {
                    property.SetColumnName(sc(property.GetColumnName()));
                }

                foreach (var key in entity.GetKeys())
                {
                    key.SetName(sc(key.GetName()));
                }

                foreach (var key in entity.GetForeignKeys())
                {
                    key.SetConstraintName(sc(key.GetConstraintName()));
                }

                foreach (var index in entity.GetIndexes())
                {
                    index.SetName(sc(index.GetName()));
                }
            }
        }
    }
}