using System;
using System.Threading.Tasks;
using Ids.AspIdentity;
using Microsoft.AspNetCore.Identity;

namespace Ids.ResetPassword
{
    public class RegisterAccountInDb : IAccountRegistration
    {
        private readonly UserManager<IdsUser> _userManager;
        public RegisterAccountInDb(
        UserManager<IdsUser> userRecords
    )
        {
            _userManager = userRecords;
        }
        public async Task<Result<UnverifiedAccount>> RegisterAccount(
            UnregisteredAccount u
        )
        {
            var user = await _userManager.FindByEmailAsync(u.Email);
            if (user != null)
            {
                var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                return new Ok<UnverifiedAccount>(new UnverifiedAccount(user.Email, user.Id, code));
            }
            else
            {
                user = new IdsUser()
                {
                    Id = Guid.NewGuid().ToString(),
                    UserName = u.Email,
                    Email = u.Email
                };
                var registration = await _userManager.CreateAsync(user);

                if (registration.Succeeded)
                {
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    return new Ok<UnverifiedAccount>(new UnverifiedAccount(user.Email, user.Id, code));
                }
                else
                {
                    return registration.ToAppError<UnverifiedAccount>();
                }
            }
        }
    }
}