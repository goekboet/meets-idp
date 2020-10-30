using System.Threading.Tasks;
using Ids.AspIdentity;
using Microsoft.AspNetCore.Identity;

namespace Ids.Unregister
{
    public class AspIdentityUnregister : IAccountDeletion
    {
        private readonly UserManager<IdsUser> _userManager;
        private readonly SignInManager<IdsUser> _signInManager;

        public AspIdentityUnregister(
            UserManager<IdsUser> userManager,
            SignInManager<IdsUser> signInManager
        )
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public async Task<Result<Unit>> Delete(ActiveAccount a)
        {
            var user = await _userManager.FindByIdAsync(a.UserId);
            if (user == null)
            {
                return new Error<Unit>($"No user with id {a.UserId} on record.");
            }
            else
            {
                var verified = await _userManager.VerifyUserTokenAsync(user, "Default", "unregister", a.Code);
                if (verified)
                {
                    var unregistration = await _userManager.DeleteAsync(user);

                    if (unregistration.Succeeded)
                    {
                        await _signInManager.SignOutAsync();

                        return new Ok<Unit>(new Unit());
                    }
                    else
                    {
                        return unregistration.ToAppError<Unit>();
                    }
                }
                else
                {
                    return new Error<Unit>("Verificaton code did not verify.");
                }
            }
            throw new System.NotImplementedException();
        }

        public async Task<Result<ActiveAccount>> Verify(
            UnverifiedAccount a)
        {
            var user = await _userManager.FindByNameAsync(a.Email);
            if (user == null)
            {
                return new Error<ActiveAccount>($"No user with username {a.Email} on record.");
            }
            else
            {
                var code = await _userManager.GenerateUserTokenAsync(user, "Default", "unregister");

                return new Ok<ActiveAccount>(new ActiveAccount(user.Id, user.Email, code));
            }
        }
    }
}