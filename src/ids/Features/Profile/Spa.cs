using System.Threading.Tasks;
using Duende.IdentityServer.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ids.Profile
{
    [Authorize]
    public class ProfileSpaController : Controller
    {
        private readonly IIdentityServerInteractionService _oidc;

        public ProfileSpaController(
            IIdentityServerInteractionService oidc
        )
        {
            _oidc = oidc;
        }

        public string v(string n) => $"~/Features/Profile/{n}.cshtml";

        [Route("/profile")]
        [HttpGet, HttpHead]
        public async Task<IActionResult> Index(string returnUrl)
        {
            var oidcLogin = await _oidc.GetAuthorizationContextAsync(returnUrl);
            
            return View(v("Index"), 
                new SpaFlags 
                { 
                    OidcLoginId = oidcLogin != null ? returnUrl : null,
                    OidcLoginName = oidcLogin?.Client?.ClientName 
                });
        }
    }
}