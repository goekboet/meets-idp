using System.Security.Claims;
using System.Threading.Tasks;
using Ids.AspIdentity;
using Microsoft.AspNetCore.Identity;

namespace Ids.Profile
{
    public class AspIdentityChangePassword : IChangePassword
    {
        private readonly UserManager<IdsUser> _userManager;

        public AspIdentityChangePassword(
            UserManager<IdsUser> userManager
        )
        {
            _userManager = userManager;
        }

        private static Claim NameClaim(string n) => new Claim("name", n);

        public async Task<Result<Unit>> Change(
            ClaimsPrincipal p, 
            string oldPwd, 
            string newPwd)
        {
            var user = await _userManager.GetUserAsync(p);
            if (user == null)
            {
                return new Error<Unit>($"Principal not on record.");
            }
            else
            {
                var changePassword = await _userManager.ChangePasswordAsync(user, oldPwd, newPwd);
                if (changePassword.Succeeded)
                {
                    return new Ok<Unit>(new Unit());
                }
                else
                {
                    return changePassword.ToAppError<Unit>();
                }
            }
        }
    }
}