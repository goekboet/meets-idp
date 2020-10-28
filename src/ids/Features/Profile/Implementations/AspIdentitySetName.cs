using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Ids.AspIdentity;
using Microsoft.AspNetCore.Identity;

namespace Ids.Profile
{
    public class AspIdentitySetName : ISetName
    {
        private readonly UserManager<IdsUser> _userManager;

        public AspIdentitySetName(
            UserManager<IdsUser> userManager
        )
        {
            _userManager = userManager;
        }

        private static Claim NameClaim(string n) => new Claim("name", n);

        public async Task<Result<Unit>> Set(ClaimsPrincipal p, string newName)
        {
            var user = await _userManager.GetUserAsync(p);
            if (user == null)
            {
                return new Error<Unit>($"Principal not on record.");
            }
            else
            {
                var claims = await _userManager.GetClaimsAsync(user);
                var currentName = claims.FirstOrDefault(x => x.Type == "name");

                IdentityResult setName;
                if (currentName == null)
                {
                    setName = await _userManager.AddClaimAsync(user, NameClaim(newName));
                }
                else
                {
                    setName = await _userManager.ReplaceClaimAsync(user, currentName, NameClaim(newName));
                }

                if (setName.Succeeded)
                {
                    return new Ok<Unit>(new Unit());
                }
                else
                {
                    return setName.ToAppError<Unit>();
                }
            }
        }
    }
}