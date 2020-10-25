using System.Threading.Tasks;
using gateway;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;

namespace IdentityServer4.Quickstart.UI
{
    public class ActivateAccountInDb : IAccountActivation
    {
        private readonly UserManager<IdsUser> _userManager;
        private readonly SignInManager<IdsUser> _signInManager;
        
        public ActivateAccountInDb(
            UserManager<IdsUser> userRecords,
            SignInManager<IdsUser> signin
        )
        {
            _userManager = userRecords;
            _signInManager = signin;
        }

        public async Task<Result<Unit>> ActivateAccount(UnverifiedAccount a)
        {
            var u = await _userManager.FindByIdAsync(a.UserId);
            if (u == null)
            {
                return new Error<Unit>("User not found.");
            }
            else
            {
                var activation = await _userManager.ConfirmEmailAsync(u, a.VerificationCode);

                if (activation.Succeeded)
                {
                    await _signInManager.SignInAsync(u, new AuthenticationProperties());
                    return new Ok<Unit>(new Unit());
                }
                else
                {
                    await _signInManager.SignOutAsync();
                    return activation.ToAppResult<Unit>();
                }
            }
        }
    }
}