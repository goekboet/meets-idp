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
        
        public async Task<Result<Invitation>> Invite(Invitation invitee)
        {
            var u = await _userManager.FindByNameAsync(invitee.Email);

            if (u == null)
            {
                var newUserId = Guid.NewGuid();
                var newUser = new IdsUser()
                {
                    Id = newUserId.ToString(),
                    UserName = invitee.Email,
                    Email = invitee.Email
                };

                var createUserResult = await _userManager.CreateAsync(newUser);
                if (createUserResult.Succeeded)
                {
                    invitee.UserId = newUserId;
                    var emptyProfile = await _userManager.AddClaimsAsync(newUser, EmptyProfile);
                    if (emptyProfile.Succeeded)
                    {
                        invitee.EmailConfirmationToken = await _userManager.GenerateEmailConfirmationTokenAsync(newUser);
                        return new Ok<Invitation>(invitee);
                    }
                    else
                    {
                        return emptyProfile.ToAppError<Invitation>();
                    }
                }
                else
                {
                    return createUserResult.ToAppError<Invitation>();
                }
            }
            else
            {
                invitee.UserId = Guid.Parse(u.Id);
                var hasPwd = await _userManager.HasPasswordAsync(u);
                if (!hasPwd)
                {
                    invitee.EmailConfirmationToken = await _userManager.GenerateEmailConfirmationTokenAsync(u);
                }
                return new Ok<Invitation>(invitee);
            }
        }
    }
}