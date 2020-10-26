using System.Threading.Tasks;
using gateway;
using Microsoft.AspNetCore.Identity;

namespace IdentityServer4.Quickstart.UI
{
    public class AspIdentityResetToken : IToken
    {
        private readonly UserManager<IdsUser> _userManager;
        
        public AspIdentityResetToken(
            UserManager<IdsUser> userManager
        )
        {
            _userManager = userManager;
        }

        public async Task<Result<ResetToken>> GetToken(
            string email)
        {
            var u = await _userManager.FindByEmailAsync(email);

            if (u == null)
            {
                return new Error<ResetToken>($"No user on record with email {email}");
            }
            else if (!u.EmailConfirmed)
            {
                return new Error<ResetToken>($"User email {email} not confirmed"); 
            }
            else
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(u);

                return new Ok<ResetToken>(new ResetToken(u.Id, u.Email, token));
            }
        }
    }
}