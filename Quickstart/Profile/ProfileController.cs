using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using gateway;
using IdentityServer4.Models;
using IdentityServer4.Services;
using IdentityServer4.Stores;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace IdentityServer4.Quickstart.UI
{
    public class ProfileUpdate
    {

        public ProfileUpdate() => new ProfileUpdate(false);

        public ProfileUpdate(bool oidclogin)
        {
            OicdLogin = oidclogin;
        }

        public bool OicdLogin { get; }

        public string ReturnUrl { get; set; }

        [Required]
        public string Name { get; set; }
        public override string ToString() => Name;

        public Claim ToNameClaim => Name != null
            ? new Claim("name", Name)
            : null;
    }

    [Authorize]
    public class ProfileController : Controller
    {
        public const string AbortIntent = "cancel";
        public const string ProceedIntent = "proceed";

        private string NameClaim => User.FindFirstValue("name");
        private string UsernameClaim => User.FindFirstValue("preferred_username");

        // In order for views to display new value
        private void UpdateRequestIdentity(Claim to)
        {
            var subj = User.Identity as ClaimsIdentity;
            subj.TryRemoveClaim(User.FindFirst(to.Type));
            subj.AddClaim(to);
        }

        private readonly IIdentityServerInteractionService _interaction;
        private readonly UserManager<IdsUser> _userManager;
        private readonly SignInManager<IdsUser> _signInManager;
        private readonly ILogger<ProfileController> _logger;
        private readonly IClientStore _clientStore;

        public ProfileController(
            IIdentityServerInteractionService interaction,
            UserManager<IdsUser> userManager,
            SignInManager<IdsUser> signInManager,
            ILogger<ProfileController> logger,
            IClientStore clientStore
        )
        {
            _interaction = interaction;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _clientStore = clientStore;
        }
        [HttpGet]
        public async Task<IActionResult> Index(string returnUrl)
        {
            var oidclogin = await _interaction
                .GetAuthorizationContextAsync(returnUrl);

            return View(new ProfileUpdate(oidclogin != null)
            {
                ReturnUrl = returnUrl,
                Name = NameClaim
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(
            ProfileUpdate update,
            string intent)
        {
            Task<AuthorizationRequest> oidc() => _interaction
                .GetAuthorizationContextAsync(update.ReturnUrl);
            try
            {
                switch (intent)
                {
                    case ProceedIntent:
                        if (ModelState.IsValid)
                        {
                            var errors = await UpdateName(update);
                            if (errors.Length == 0)
                            {
                                var oidcProceed = await oidc();
                                if (oidcProceed != null)
                                {
                                    return Redirect(update.ReturnUrl);
                                }
                                else
                                {
                                    return View("Index", update);
                                }
                            }
                            else
                            {
                                foreach (var e in errors)
                                {
                                    ModelState.TryAddModelError(e.k, e.v);
                                }

                                return View("Index", update);
                            }
                        }
                        else
                        {
                            return View("Index", update);
                        }

                    case AbortIntent:
                        var abortOidc = await oidc();

                        if (abortOidc != null)
                        {
                            return await AbortOidc(abortOidc, update.ReturnUrl);
                        }
                        else
                        {
                            return update.ReturnUrl != null
                                ? Redirect(update.ReturnUrl) as IActionResult
                                : View("Index") as IActionResult;
                        }
                    default:
                        _logger.LogWarning($"Bad intent: {intent}");
                        ModelState.TryAddModelError("Query", "Bad query. Do not retry.");
                        return View("Index", update);
                }
            }
            catch (Exception e)
            {
                _logger.LogError("Service failure while processing post with {intent}", e, intent);
                ModelState.TryAddModelError("Update", "Service temporary unable to comply");

                return View("Index", update);
            }
        }

        private async Task<IActionResult> AbortOidc(
            AuthorizationRequest oidc,
            string returnUrl)
        {
            await _interaction.GrantConsentAsync(oidc, ConsentResponse.Denied);

            return Redirect(returnUrl);
        }

        private async Task<(string k, string v)[]> UpdateName(ProfileUpdate update)
        {
            var userRecord = await _userManager.GetUserAsync(User);
            var claimsrecord = await _userManager.GetClaimsAsync(userRecord);
            var current = claimsrecord.FirstOrDefault(x => x.Type == "name");

            IdentityResult r = (current != null)
                ? await _userManager.ReplaceClaimAsync(userRecord, current, update.ToNameClaim)
                : await _userManager.AddClaimAsync(userRecord, update.ToNameClaim);

            if (r.Succeeded)
            {
                await _signInManager.RefreshSignInAsync(userRecord);
                UpdateRequestIdentity(update.ToNameClaim);

                _logger.LogInformation("Updated name-claim for {UserId} from {current} to {New}", UsernameClaim, current?.Value ?? "n/a", NameClaim);

                return new (string, string)[0];
            }
            else
            {
                return r.Errors.Select(x => (x.Code, x.Description)).ToArray();
            }
        }
    }
}