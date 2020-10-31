using System.Threading.Tasks;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Mvc;

namespace Ids.Login
{
    public class LoginController : Controller
    {
        private readonly IVerifyCredentials _login;
        private readonly IIdentityServerInteractionService _interaction;

        public LoginController(
            IVerifyCredentials login,
            IIdentityServerInteractionService interaction
        )
        {
            _login = login;
            _interaction = interaction;
        }

        string v(string name) => $"~/Features/Login/Views/{name}.cshtml";

        [HttpGet, HttpHead]
        public IActionResult Index(string returnUrl)
        {
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