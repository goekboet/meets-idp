using System.Threading.Tasks;
using Duende.IdentityServer.Services;
using Ids.AspIdentity;
using Ids.Invite;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Ids.Login
{
    public class LoginController : Controller
    {
        private readonly IVerifyCredentials _login;
        private readonly IIdentityServerInteractionService _interaction;
        private readonly IInvitation _invitation;
        UserManager<IdsUser> UserManager { get; }

        public LoginController(
            IVerifyCredentials login,
            IIdentityServerInteractionService interaction,
            IInvitation invitation,
            UserManager<IdsUser> usermanager
        )
        {
            _login = login;
            _interaction = interaction;
            _invitation = invitation;
            UserManager = usermanager;
        }

        string v(string name) => $"~/Features/Login/Views/{name}.cshtml";

        [HttpGet, HttpHead]
        public async Task<IActionResult> Index(string returnUrl)
        {
            var oidcContext = await _interaction.GetAuthorizationContextAsync(returnUrl);
            var idp = oidcContext?.IdP;
            if (idp == "github")
            {
                var props = new AuthenticationProperties
                {
                    RedirectUri = "/nonlocal/callback"
                };
                props.Items.Add("returnUrl", returnUrl);

                return Challenge(props, "github");
            }
            var userNameHint = oidcContext?.LoginHint;
            
            if ((idp == null || idp == "local") && userNameHint != null)
            {
                var (record, status) = await UserManager.GetUserRecordStatus(userNameHint);
                switch (status)
                {
                    case UserRecordStatus.Unknown:
                        return RedirectToAction(
                        "Index",
                        "Register",
                        new
                        {
                            ReturnUrl = returnUrl,
                            Email = userNameHint
                        });
                    case UserRecordStatus.AwaitingEmailConfirmation:
                        return RedirectToAction(
                        "Verify",
                        "Register",
                        new
                        {
                            UserId = record.Id,
                            ReturnUrl = returnUrl
                        });
                    default:
                        return View(v("Index"), new LoginInput { ReturnUrl = returnUrl });
                }
            }

            return View(v("Index"), new LoginInput { ReturnUrl = returnUrl });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyCredentials(
            LoginInput i
        )
        {
            if (ModelState.IsValid)
            {
                var login = await _login.Verify(i.Email, i.Password);
                if (login is Ok<Unit> verifyOk)
                {
                    var oidcContext = await _interaction.GetAuthorizationContextAsync(i.ReturnUrl);

                    return Redirect(oidcContext != null ? i.ReturnUrl : "/");
                }
                else if (login is Error<Unit> verifyErr)
                {
                    if (verifyErr.Description == "InvalidCreds")
                    {
                        ModelState.AddModelError("Email", "Login failed. Please double-check.");
                    }
                    else if (verifyErr.Description == "LockedOut")
                    {
                        ModelState.AddModelError("Password", "Too many attempts. Account locked for a short time.");
                    }
                }
                return View(v("Index"), i);
            }
            else
            {
                return View(v("Index"), i);
            }
        }
    }
}