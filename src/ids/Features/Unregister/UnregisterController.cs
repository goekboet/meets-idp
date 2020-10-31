using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Ids.Unregister
{
    [SecurityHeaders]
    [AllowAnonymous]
    public class UnregisterController : Controller
    {
        private IAccountDeletion Account { get; }
        private ICodeDistribution Code { get; }
        private ILogger<UnregisterController> Log { get; }

        public UnregisterController(
            IAccountDeletion account,
            ICodeDistribution codeDistribution,
            ILogger<UnregisterController> log
        )
        {
            Account = account;
            Code = codeDistribution;
            Log = log;
        }

        string v(string name) => $"~/Features/Unregister/Views/{name}.cshtml";

        [HttpGet, HttpHead]
        public IActionResult Index(string returnUrl)
        {
            return View(v("Index"), new OidcLogin { ReturnUrl = returnUrl });
        }

        string CodeFromResult(Result<string> r) => r is Ok<string> okR ? okR.Value : "";

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RequestEmailVerification(
            OidcLogin login
        )
        {
            if (ModelState.IsValid)
            {
                var verification = await Account.Verify(User);
                if (verification is Ok<ActiveAccount> ok)
                {
                    var c = await Code.Send(HttpContext.Response, ok.Value);
                    
                    return RedirectToAction("Verify", "Unregister", new { code = CodeFromResult(c) });
                }
                else
                {
                    if (verification is Error<ActiveAccount> err)
                    {
                        Log.LogError(err.Description);
                    }

                    ModelState.AddModelError("", "Technical error. Please try again later.");
                    return View(v("Index"), login);
                }
            }
            else
            {
                return View(v("Index"), login);
            }

        }

        [HttpGet, HttpHead]
        public IActionResult Verify(
            string userId,
            string code
        )
        {
            return View(v("Verify"), new EmailVerificationCode() { Code = code });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(
            EmailVerificationCode r
        )
        {
            if (ModelState.IsValid)
            {
                var activation = await Account.Delete(User, r.Code);
                if (activation is Ok<Unit> ok)
                {
                    return RedirectToAction("AccountDeleted");
                }
                else
                {
                    if (activation is Error<Unit> err)
                    {
                        Log.LogError(err.Description);
                    }
                    ModelState.AddModelError("Code", "Not acepted. Please double-check.");

                    return View(v("Verify"), r);
                }

            }
            else
            {
                return View(v("Verify"), r);
            }

        }

        [HttpGet, HttpHead]
        [Authorize]
        public IActionResult AccountDeleted()
        {
            return View(v("AccountDeleted"));
        }

        [HttpGet, HttpHead]
        public IActionResult AccountDeletionFailed()
        {
            return View(v("AccountDeletionFailed"));
        }
    }
}