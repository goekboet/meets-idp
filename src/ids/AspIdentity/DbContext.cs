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
    }
}