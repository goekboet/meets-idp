using System.Threading.Tasks;
using Duende.IdentityServer;
using Ids.AspIdentity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Duende.IdentityServer.Services;
using Duende.IdentityServer.Events;

namespace Ids.NonLocal
{
    public class NonLocalController : Controller
    {
        ILogger<NonLocalController> Logger { get; }
        UserManager<IdsUser> UserManager { get; }
        IIdentityServerInteractionService Oidc { get; }
        IEventService Events { get; }

        public NonLocalController(
            ILogger<NonLocalController> logger,
            UserManager<IdsUser> userManager,
            IEventService events,
            IIdentityServerInteractionService oidc
        )
        {
            Logger = logger;
            UserManager = userManager;
            Events = events;
            Oidc = oidc;
        }

        [HttpGet]
        public async Task<IActionResult> Callback()
        {
            var result = await HttpContext.AuthenticateAsync(IdentityServerConstants.ExternalCookieAuthenticationScheme);
            if (result?.Succeeded ?? false)
            {
                var nonLocalUser = result.Principal.ToNonLocalUser();
                var localUser = await UserManager.RecordNonLocalUser(nonLocalUser);

                var props = new AuthenticationProperties();
                var idsUser = new IdentityServerUser(localUser.Id)
                {
                    DisplayName = nonLocalUser.name,
                    IdentityProvider = result.Properties.Items[".AuthScheme"],
                };

                await HttpContext.SignInAsync(idsUser, props);
                await HttpContext.SignOutAsync(IdentityServerConstants.ExternalCookieAuthenticationScheme);

                var returnUrl = result.Properties.Items["returnUrl"] ?? "~/";
                var oidcCtx = await Oidc.GetAuthorizationContextAsync(returnUrl);
                await Events.RaiseAsync(new UserLoginSuccessEvent(
                    provider: nonLocalUser.issuer,
                    providerUserId: nonLocalUser.id,
                    idsUser.SubjectId,
                    name: nonLocalUser.name,
                    interactive: true,
                    clientId: oidcCtx?.Client.ClientId
                ));

                return Redirect(returnUrl);
            }
            else
            {
                Logger.LogWarning($"Not signed in via {IdentityServerConstants.ExternalCookieAuthenticationScheme}");
                return BadRequest();
            }
        }
    }
}