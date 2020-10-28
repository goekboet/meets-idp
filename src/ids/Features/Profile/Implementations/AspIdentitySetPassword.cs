using System.Security.Claims;
using System.Threading.Tasks;
using Ids.AspIdentity;
using Microsoft.AspNetCore.Identity;

namespace Ids.Profile
{
    public class AspIdentitySetPassword : ISetPassword
    {
        private readonly UserManager<IdsUser> _userManager;

        public AspIdentitySetPassword(
            UserManager<IdsUser> userManager
        )
        {
            _userManager = userManager;
        }

        public async Task<Result<Unit>> Set(ClaimsPrincipal p, string newPassword)
        {
            var user = await _userManager.GetUserAsync(p);
            if (user == null)
            {
                return new Error<Unit>($"Principal not on record.");
            }
            else
            {
                var setPwd = await _userManager.AddPasswordAsync(user, newPassword);

                if (setPwd.Succeeded)
                {
                    return new Ok<Unit>(new Unit());
                }
                else
                {
                    return setPwd.ToAppError<Unit>();
                }
            }
        }
    }
}