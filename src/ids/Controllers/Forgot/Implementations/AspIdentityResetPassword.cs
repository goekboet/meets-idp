using System.Threading.Tasks;
using gateway;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;

namespace IdentityServer4.Quickstart.UI
{
    public class AspIdentityResetPassword : IReset
    {
        private readonly UserManager<IdsUser> _userManager;
        private readonly SignInManager<IdsUser> _signInManager;

        public AspIdentityResetPassword(
            UserManager<IdsUser> userManager,
            SignInManager<IdsUser> signinManager
        )
        {
            _userManager = userManager;
            _signInManager = signinManager;
        }

        public async Task<Result<Unit>> Reset(VerifiedReset r)
        {
            var u = await _userManager.FindByIdAsync(r.UserId);

            if (u == null)
            {
                return new Error<Unit>($"No user with id {r.UserId} on record");
            }
            else
            {
                var reset = await _userManager.ResetPasswordAsync(u, r.Token, r.Password);
                if (reset.Succeeded)
                {
                    await _signInManager.SignInAsync(u, new AuthenticationProperties());
                    return new Ok<Unit>(new Unit());
                }
                else
                {
                    await _signInManager.SignOutAsync();
                    return reset.ToAppError<Unit>();
                }
            }
        }
    }
}