using System.Threading.Tasks;
using Duende.IdentityServer.Services;
using Ids.Invite;
using Microsoft.AspNetCore.Mvc;

namespace Ids.Login
{
    public class LoginController : Controller
    {
        private readonly IVerifyCredentials _login;
        private readonly IIdentityServerInteractionService _interaction;
        private readonly IInvitation _invitation;

        public LoginController(
            IVerifyCredentials login,
            IIdentityServerInteractionService interaction,
            IInvitation invitation
        )
        {
            _login = login;
            _interaction = interaction;
            _invitation = invitation;
        }

        string v(string name) => $"~/Features/Login/Views/{name}.cshtml";

        [HttpGet, HttpHead]
        public async Task<IActionResult> Index(string returnUrl)
        {
            var oidcContext = await _interaction.GetAuthorizationContextAsync(returnUrl);
            var userNameHint = oidcContext.LoginHint;
            if (userNameHint != null)
            {
                var status = await _invitation.GetInvitationStatus(userNameHint);
                if (!status.Registered)
                {
                    return RedirectToAction(
                        "Index",
                        "Register",
                        new
                        {
                            ReturnUrl = returnUrl,
                            Email = userNameHint
                        });
                }
                else if (!status.HasPassword)
                {
                    return RedirectToAction(
                        "Verify",
                        "Register",
                        new
                        {
                            UserId = status.UserId,
                            ReturnUrl = returnUrl
                        }
                    );
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