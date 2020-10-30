using System.Security.Claims;
using System.Threading.Tasks;
using Ids.AspIdentity;
using Microsoft.AspNetCore.Identity;

namespace Ids.ChangeUsername
{
    public class AspIdentityToken : IToken
    {
        private readonly UserManager<IdsUser> _userManager;

        public AspIdentityToken(
            UserManager<IdsUser> userManager
        )
        {
            _userManager = userManager;
        }
        
        public async Task<Result<ResetToken>> GetToken(ClaimsPrincipal p, string newEmail)
        {
            var existingUser = await _userManager.FindByNameAsync(newEmail);
            if (existingUser != null)
            {
                return new Error<ResetToken>("Email already taken.");
            }

            var user = await _userManager.GetUserAsync(p);
            if (user == null)
            {
                return new Error<ResetToken>($"Claimsprincipal not on record.");
            }
            else
            {
                var t = await _userManager.GenerateChangeEmailTokenAsync(user, newEmail);

                return new Ok<ResetToken>(new ResetToken(user.Id, newEmail, t));
            }
        }
    }
}