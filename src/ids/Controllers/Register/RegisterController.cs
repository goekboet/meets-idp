using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace IdentityServer4.Quickstart.UI
{
    [SecurityHeaders]
    [AllowAnonymous]
    public class RegisterController : Controller
    {
        private IAccountRegistration Registration { get; }
        private IAccountActivation Activation { get; }
        private ICodeDistribution Code { get; }

        private ILogger<RegisterController> Log { get;}
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

        [HttpGet, HttpHead]
        public IActionResult Index()
        {
            return View(new RegistrationRequest());
        }

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
                    Response.OnCompleted(async () => {
                        await Code.Send(ok.Value);
                    });
                    return RedirectToAction("Verify", "Register", new { userId = ok.Value.UserId });
                }
                else 
                {
                    if (registration is Error<UnverifiedAccount> err)
                    {
                        Log.LogError(err.Description);
                    }

                    ModelState.AddModelError("", "Technical error. Please try again later.");
                    return View(r);
                }
            }
            else
            {
                return View(r);
            }

        }

        [HttpGet, HttpHead]
        public IActionResult Verify(
            string userId
        )
        {
            return View(new EmailVerificationCode() { UserId = userId });
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
                    return RedirectToAction("EmailVerified");
                }
                else
                {
                    if (activation is Error<Unit> err)
                    {
                        Log.LogError(err.Description);
                    }
                    
                    return RedirectToAction("EmailVerificationFailed");
                }
                
            }
            else
            {
                return View("Verify", r);
            }

        }

        [HttpGet, HttpHead]
        [Authorize]
        public IActionResult EmailVerified()
        {
            return View();
        }

        [HttpGet, HttpHead]
        public IActionResult EmailVerificationFailed()
        {
            return View();
        }
    }
}