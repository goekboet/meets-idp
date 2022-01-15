using System;
using System.Linq;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Ids.AspIdentity;
using Microsoft.AspNetCore.Identity;

namespace Ids.NonLocal
{
    public record NonLocalUser(string id, string name, string email, string issuer);

    public static class NonLocalExtensions
    {
        public static NonLocalUser ToNonLocalUser(
            this ClaimsPrincipal p
        )
        {
            return new NonLocalUser(
                id: p.FindFirstValue(ClaimTypes.NameIdentifier),
                name: p.FindFirstValue(ClaimTypes.Name),
                email: p.FindFirstValue(ClaimTypes.Email),
                issuer: p.Identity.AuthenticationType
            );
        }

        const string emailTagPattern = @"\+[^+|@]*";
        static string UnTagEmail(string taggedEmail) => Regex.Replace(taggedEmail, emailTagPattern, "");

        public static async Task<IdsUser> RecordNonLocalUser(
            this UserManager<IdsUser> userManager,
            NonLocalUser user
        )
        {
            var localUser = await userManager.FindByLoginAsync(user.issuer, user.id);
            var untaggedEmail = UnTagEmail(user.email);
            if (localUser is null)
            {
                localUser = await userManager.FindByNameAsync(untaggedEmail);
                if (localUser == null)
                {
                    localUser = new IdsUser
                    {
                        Id = Guid.NewGuid().ToString(),
                        UserName = untaggedEmail,
                        Email = untaggedEmail
                    };

                    var createUserResult = await userManager.CreateAsync(localUser);
                    if (!createUserResult.Succeeded)
                    {
                        var msgs = string.Join(", ", createUserResult.Errors.Select(x => $"{x.Code} {x.Description}"));
                        throw new Exception($"Unable to create a local user for non-local user. {msgs}");
                    }
                }

                var login = new UserLoginInfo(user.issuer, user.id, user.issuer);
                var addLoginResult = await userManager.AddLoginAsync(localUser, login);
                if (!addLoginResult.Succeeded)
                {
                    var msgs = string.Join(", ", addLoginResult.Errors.Select(x => $"{x.Code} {x.Description}"));
                    throw new Exception($"Unable to add login. {msgs}");
                }

                var addNameClaimResult = await userManager.AddClaimAsync(localUser, new Claim("name", user.name));
                if (!addNameClaimResult.Succeeded)
                {
                    var msgs = string.Join(", ", addNameClaimResult.Errors.Select(x => $"{x.Code} {x.Description}"));
                    throw new Exception($"Unable to add name-claim. {msgs}");
                }
            }

            return localUser;
        }
    }
}