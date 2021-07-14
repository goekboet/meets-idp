using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Ids.Register
{
    [SecurityHeaders]
    [AllowAnonymous]
    public class RegisterController : Controller
    {
        private IAccountRegistration Registration { get; }
        private IAccountActivation Activation { get; }
        private ICodeDistribution Code { get; }

        private ILogger<RegisterController> Log { get; }
        public RegisterController(
            IAccountRegistration registration,
            IAccountActivation activation,
            ICodeDistribution codeDistribution,
            ILogger<RegisterController> log
        )
        {
            Registration = registration;
            Activation = activation;
            Code = codeDistribution;
            Log = log;
        }

        string v(string name) => $"~/Features/Register/Views/{name}.cshtml";

        [HttpGet, HttpHead]
        public IActionResult Index(string returnUrl, string userNameHint)
        {
            return View(
                v("Index"),
                new RegistrationRequest()
                {
                    Email = userNameHint,
                    ReturnUrl = returnUrl
                });
        }

        string CodeFromResult(Result<string> r) => r is Ok<string> okR ? okR.Value : "";

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RequestEmailVerification(
            RegistrationRequest r
        )
        {
            if (ModelState.IsValid)
            {
                var registration = await Registration.RegisterAccount(new UnregisteredAccount(r.Email));
                if (registration is Ok<UnverifiedAccount> ok)
                {
                    var c = await Code.Send(HttpContext.Response, ok.Value);

                    return RedirectToAction("Verify", "Register",
                        new
                        {
                            userId = ok.Value.UserId,
                            code = CodeFromResult(c),
                            returnUrl = r.ReturnUrl
                        });
                }
                else
                {
                    if (registration is Error<UnverifiedAccount> err)
                    {
                        Log.LogError(err.Description);
                    }

                    ModelState.AddModelError("Email", "Not accepted. This email might be taken.");
                    return View(v("Index"), r);
                }
            }
            else
            {
                return View(v("Index"), r);
            }

        }

        [HttpGet, HttpHead]
        public IActionResult Verify(
            string userId,
            string code,
            string returnUrl
        )
        {
            return View(v("Verify"),
            new EmailVerificationCode()
            {
                UserId = userId,
                Code = code,
                ReturnUrl = returnUrl
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyCode(
            EmailVerificationCode r
        )
        {
            if (ModelState.IsValid)
            {
                var activation = await Activation.ActivateAccount(new UnverifiedAccount("", r.UserId, r.Code));
                if (activation is Ok<Unit> ok)
                {
                    return RedirectToAction("Index", "ProfileSpa", new { returnUrl = r.ReturnUrl });
                }
                else
                {
                    if (activation is Error<Unit> err)
                    {
                        Log.LogError(err.Description);
                    }
                    ModelState.AddModelError("Code", "Not accepted. Please double-check.");

                    return View(v("Verify"), r);
                }

            }
            else
            {
                return View(v("Verify"), r);
            }

        }
    }
}