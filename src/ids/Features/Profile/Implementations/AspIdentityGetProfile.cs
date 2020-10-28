using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Ids.AspIdentity;
using Microsoft.AspNetCore.Identity;

namespace Ids.Profile
{
    public class AspIdentityGetProfile : IGetProfile
    {
        private readonly UserManager<IdsUser> _userManager;

        public AspIdentityGetProfile(
            UserManager<IdsUser> userManager
        )
        {
            _userManager = userManager;
        }
        public async Task<Result<Profile>> Get(ClaimsPrincipal p)
        {
            var user = await _userManager.GetUserAsync(p);
            if (user == null)
            {
                return new Error<Profile>($"Provided principal not on record.");
            }
            else
            {
                var claims = await _userManager.GetClaimsAsync(user);
                var hasPwd = await _userManager.HasPasswordAsync(user);

                var profile = new Profile(
                    name: claims.FirstOrDefault(x => x.Type == "name")?.Value,
                    email: user.Email,
                    hasPassword: hasPwd
                );

                return new Ok<Profile>(profile);
            }
        }
    }
}