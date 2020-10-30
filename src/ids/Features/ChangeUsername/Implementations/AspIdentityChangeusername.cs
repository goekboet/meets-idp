using System.Security.Claims;
using System.Threading.Tasks;
using Ids.AspIdentity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;

namespace Ids.ChangeUsername
{
    public class AspIdentityChangeUsername : IUsername
    {
        private readonly UserManager<IdsUser> _userManager;
        private readonly SignInManager<IdsUser> _signInManager;

        public AspIdentityChangeUsername(
            UserManager<IdsUser> userManager,
            SignInManager<IdsUser> signInManager
        )
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }
        
        public async Task<Result<Unit>> Change(
            ClaimsPrincipal p,
            VerifiedChangeUsername r)
        {
            var existingUser = await _userManager.FindByNameAsync(r.NewUsername);
            if (existingUser != null)
            {
                return new Error<Unit>("Email already taken.");
            }

            var user = await _userManager.GetUserAsync(p);
            if (user == null)
            {
                return new Error<Unit>($"Claimsprincipal not on record.");
            }
            else
            {
                var changeEmail = await _userManager.ChangeEmailAsync(user, r.NewUsername, r.Token);

                if (changeEmail.Succeeded)
                {
                    var changeUsername = await _userManager.SetUserNameAsync(user, r.NewUsername);

                    if (changeUsername.Succeeded)
                    {
                        await _signInManager.SignInAsync(user, new AuthenticationProperties());
                        return new Ok<Unit>(new Unit());
                    }
                    else
                    {
                        return new Error<Unit>($"Username update failed");
                    }
                }
                else
                {
                    return changeEmail.ToAppError<Unit>();
                }
            }
            throw new System.NotImplementedException();
        }
    }
}