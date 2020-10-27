using System.Threading.Tasks;
using Ids.AspIdentity;
using Microsoft.AspNetCore.Identity;

namespace Ids.Login
{
    public class AspIdentityLogin : IVerifyCredentials
    {
        private readonly UserManager<IdsUser> _userManager;
        private readonly SignInManager<IdsUser> _signInManager;
        
        public AspIdentityLogin(
            UserManager<IdsUser> userManager,
            SignInManager<IdsUser> signInManager
        )
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public async Task<Result<Unit>> Verify(string userName, string password)
        {
            var login = await _signInManager.PasswordSignInAsync(userName, password, false, true);
            if (login.Succeeded)
            {
                return new Ok<Unit>(new Unit());
            }
            else
            {
                await _signInManager.SignOutAsync();
                return new Error<Unit>("Login failed");
            }
        }
    }
}