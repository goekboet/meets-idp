using System;
using System.Collections.Generic;
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
    public class ProfileClaim
    {
        public static bool MapsClaimName(string n) => 
            IdsNames.Keys.Contains(n);

        public static string MapClaimName(string n) =>
            MapsClaimName(n) ? IdsNames[n] : n;
        public static Dictionary<string, string> IdsNames = new Dictionary<string, string>
        {
            ["name"] = "Friendly name",
            ["family_name"] = "family_name",
            ["given_name"] = "given_name",
            ["middle_name"] = "middle_name",
            ["nickname"] = "nickname",
            ["preferred_username"] = "Account name",
            ["profile"] = "profile",
            ["picture"] = "picture",
            ["website"] = "website",
            ["gender"] = "gender",
            ["birthdate"] = "birthdate",
            ["zoneinfo"] = "zoneinfo",
            ["locale"] = "locale",
            ["updated_at"] = "updated_at"
        };
        public string Name { get; set; }
        public string Value { get; set; }
    }

    public class ProfileUpdate
    {

        public ProfileUpdate() => new ProfileUpdate(
            false,
            new ProfileClaim[0]);

        public ProfileUpdate(
            bool oidclogin,
            ProfileClaim[] claims)
        {
            Claims = claims ?? new ProfileClaim[0];
            OicdLogin = oidclogin;
        }

        public ProfileClaim[] Claims { get; }

        public bool OicdLogin { get; }

        public string ReturnUrl { get; set; }

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
            var oidc = await _interaction
                .GetAuthorizationContextAsync(returnUrl);

            var q = from c in User.Claims
                    where ProfileClaim.MapsClaimName(c.Type)
                    select new ProfileClaim
                    {
                        Name = ProfileClaim.MapClaimName(c.Type),
                        Value = c.Value
                    };

            return View(new ProfileUpdate(
                oidclogin: oidc != null,
                claims: q.ToArray())
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
                        if (update.ToNameClaim is Claim n)
                        {
                            var errors = await UpdateName(n);
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

        private async Task<(string k, string v)[]> UpdateName(Claim update)
        {
            var userRecord = await _userManager.GetUserAsync(User);
            var claimsrecord = await _userManager.GetClaimsAsync(userRecord);
            var current = claimsrecord.FirstOrDefault(x => x.Type == "name");

            IdentityResult r = (current != null)
                ? await _userManager.ReplaceClaimAsync(userRecord, current, update)
                : await _userManager.AddClaimAsync(userRecord, update);

            if (r.Succeeded)
            {
                await _signInManager.RefreshSignInAsync(userRecord);
                UpdateRequestIdentity(update);

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