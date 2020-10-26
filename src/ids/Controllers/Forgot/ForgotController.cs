using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace IdentityServer4.Quickstart.UI
{
    [SecurityHeaders]
    [AllowAnonymous]
    public class ForgotController : Controller
    {
        private readonly IToken _token;
        private readonly IExchange _exchange;
        private readonly IReset _reset;
        private readonly ILogger<ForgotController> _logger;
        

        public ForgotController(
            IToken token,
            IExchange exchange,
            IReset reset,
            ILogger<ForgotController> logger
        )
        {
            _token = token;
            _exchange = exchange;
            _reset = reset;
            _logger = logger;
        }

        [HttpGet, HttpHead]
        public IActionResult Index()
        {
            return View(new ResetInput());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reset(ResetInput r)
        {
            if (ModelState.IsValid)
            {
                var token = await _token.GetToken(r.Email);
                if (token is Ok<ResetToken> okToken)
                {
                    await _exchange.Exchange(okToken.Value);
                    return RedirectToAction("Token", new { userId = okToken.Value.UserId });
                }
                else
                {
                    if (token is Error<ResetToken> errToken)
                    {
                        _logger.LogError(errToken.Description);
                    }
                    ModelState.AddModelError("", "Technical error. Are you sure you have an account with this email? Please try again later.");

                    return View("Index", r);
                }     
            }
            else
            {
                return View(r);
            }
        }

        [HttpGet, HttpHead]
        public IActionResult Token(string userId)
        {
            return View(new VerifiedResetInput { UserId = userId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Verify(VerifiedResetInput c)
        {
            if (ModelState.IsValid)
            {
                var reset = await _reset.Reset(new VerifiedReset(c.UserId, c.Code, c.Password));
                if (reset is Ok<Unit> resetOk)
                {
                    return RedirectToAction("Verified");
                }
                else
                {
                    if (reset is Error<Unit> resetErr)
                    {
                        _logger.LogError(resetErr.Description);
                    }

                    return RedirectToAction("VerificationFailed");
                }        
            }
            else
            {
                return View("Token", c);
            }
        }

        [Authorize]
        [HttpGet, HttpHead]
        public IActionResult Verified()
        {
            return View();
        }

        [HttpGet, HttpHead]
        public IActionResult VerificationFailed()
        {
            return View();
        }
    }
}