using System.Collections.Generic;
using System.Threading.Tasks;
using IdentityModel;
using IdentityServer4;
using IdentityServer4.Events;
using IdentityServer4.Models;
using IdentityServer4.Services;
using IdentityServer4.Validation;
using Ids.AspIdentity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using static IdentityModel.OidcConstants;

namespace Ids.AspIdentity
{
    public class Profile : IdentityResource
    {
        public static IEnumerable<string> SupportedProfileClaims = new []
        {
            JwtClaimTypes.Name,
            JwtClaimTypes.UpdatedAt
        };
        /// <summary>
        /// Initializes a new instance of the <see cref="Profile"/> class.
        /// </summary>
        public Profile()
        {
            Name = IdentityServerConstants.StandardScopes.Profile;
            DisplayName = "Profile";
            Description = "Yout birthname, basically";
            Emphasize = true;
            UserClaims = SupportedProfileClaims as ICollection<string>;
        }
    }
    /// <summary>
    /// Cut and pasted from IdentityServer4.AspNetIdentity only changed
    /// to lookup user via email instead of username.
    /// </summary>
    public class MapUsernameToEmail : IResourceOwnerPasswordValidator
    {
        private readonly UserManager<IdsUser> _userManager;
        private readonly ILogger<MapUsernameToEmail> _logger;
        private readonly SignInManager<IdsUser> _signInManager;
        private IEventService _events;

        public MapUsernameToEmail(
            UserManager<IdsUser> userManager,
            ILogger<MapUsernameToEmail> logger,
            SignInManager<IdsUser> signInManager,
            IEventService events)
        {
            _userManager = userManager;
            _logger = logger;
            _signInManager = signInManager;
            _events = events;
        }

        public async Task ValidateAsync(
            ResourceOwnerPasswordValidationContext context)
        {
            var clientId = context.Request?.Client?.ClientId;
            var user = await _userManager.FindByEmailAsync(context.UserName);
            if (user != null)
            {
                var result = await _signInManager.CheckPasswordSignInAsync(user, context.Password, true);
                if (result.Succeeded)
                {
                    var sub = await _userManager.GetUserIdAsync(user);

                    _logger.LogInformation("Credentials validated for email: {username}", context.UserName);
                    await _events.RaiseAsync(new UserLoginSuccessEvent(context.UserName, sub, context.UserName, false, clientId));

                    context.Result = new GrantValidationResult(sub, AuthenticationMethods.Password);
                    return;
                }
                else if (result.IsLockedOut)
                {
                    _logger.LogInformation("Authentication failed for email: {username}, reason: locked out", context.UserName);
                    await _events.RaiseAsync(new UserLoginFailureEvent(context.UserName, "locked out", false, clientId));
                }
                else if (result.IsNotAllowed)
                {
                    _logger.LogInformation("Authentication failed for email: {username}, reason: not allowed", context.UserName);
                    await _events.RaiseAsync(new UserLoginFailureEvent(context.UserName, "not allowed", false, clientId));
                }
                else
                {
                    _logger.LogInformation("Authentication failed for email: {username}, reason: invalid credentials", context.UserName);
                    await _events.RaiseAsync(new UserLoginFailureEvent(context.UserName, "invalid credentials", false, clientId));
                }
            }
            else
            {
                _logger.LogInformation("No user found matching email: {username}", context.UserName);
                await _events.RaiseAsync(new UserLoginFailureEvent(context.UserName, "invalid username", false, clientId));
            }

            context.Result = new GrantValidationResult(TokenRequestErrors.InvalidGrant);
        }
    }
}