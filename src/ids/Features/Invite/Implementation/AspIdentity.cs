using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Ids.AspIdentity;
using Microsoft.AspNetCore.Identity;

namespace Ids.Invite
{
    public class AspIdentityInvitation : IInvitation
    {
        private readonly UserManager<IdsUser> _userManager;

        public AspIdentityInvitation(
            UserManager<IdsUser> userManager
        )
        {
            _userManager = userManager;
        }

        public static IEnumerable<Claim> EmptyProfile =>
            new[]
            {
                new Claim("name", string.Empty ),
                new Claim("picture", string.Empty )
            };
        
        public async Task<Result<Guid>> Invite(string email)
        {
            var u = await _userManager.FindByEmailAsync(email);

            if (u == null)
            {
                var newUserId = Guid.NewGuid();
                var newUser = new IdsUser()
                {
                    Id = newUserId.ToString(),
                    UserName = email,
                    Email = email
                };

                var createUserResult = await _userManager.CreateAsync(newUser);
                if (createUserResult.Succeeded)
                {
                    var emptyProfile = await _userManager.AddClaimsAsync(newUser, EmptyProfile);
                    if (emptyProfile.Succeeded)
                    {
                        return new Ok<Guid>(newUserId);
                    }
                    else
                    {
                        return emptyProfile.ToAppError<Guid>();
                    }
                }
                else
                {
                    return createUserResult.ToAppError<Guid>();
                }
            }
            else
            {
                return new Ok<Guid>(Guid.Parse(u.Id));
            }
        }
    }
}