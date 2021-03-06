using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Ids.ChangeUsername
{
    [SecurityHeaders]
    [Authorize]
    public class ChangeUsernameController : Controller
    {
        private readonly IToken _token;
        private readonly IExchange _exchange;
        private readonly IUsername _username;
        private readonly ILogger<ChangeUsernameController> _logger;
        
        string v(string name) => $"~/Features/ChangeUsername/Views/{name}.cshtml";

        public ChangeUsernameController(
            IToken token,
            IExchange exchange,
            IUsername username,
            ILogger<ChangeUsernameController> logger
        )
        {
            _token = token;
            _exchange = exchange;
            _username = username;
            _logger = logger;
        }

        string CodeFromResult(Result<string> r) => r is Ok<string> okR ? okR.Value : "";

        string CurrentEmail() => HttpContext.User.FindFirst("email")?.Value ?? "";

        [HttpGet, HttpHead]
        public IActionResult Index(string returnUrl)
        {
            return View(
                v("Index"), 
                new ChangeUsernameInput 
                {
                    CurentUsername = CurrentEmail(), 
                    ReturnUrl = returnUrl
                });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Verify(ChangeUsernameInput r)
        {
            if (ModelState.IsValid)
            {
                var token = await _token.GetToken(User, r.NewUsername);
                if (token is Ok<ResetToken> okToken)
                {
                    var t = await _exchange.Exchange(Response, okToken.Value);
                    return RedirectToAction("Token", 
                    new 
                    { 
                        code = CodeFromResult(t), 
                        newEmail = r.NewUsername,
                        returnUrl = r.ReturnUrl
                    });
                }
                else
                {
                    if (token is Error<ResetToken> errToken)
                    {
                        _logger.LogError(errToken.Description);
                    }
                    ModelState.AddModelError("NewUsername", "Unable to change to this email.");

                    return View(v("Index"), r);
                }     
            }
            else
            {
                return View(v("Index"), r);
            }
        }

        [HttpGet, HttpHead]
        public IActionResult Token( 
            string code,
            string newEmail,
            string returnUrl)
        {
            return View(v("Token"), 
            new VerifiedChangeUsernameInput 
            {
                CurentUsername = CurrentEmail(), 
                Code = code, 
                NewUsername = newEmail,
                ReturnUrl = returnUrl
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Change(VerifiedChangeUsernameInput c)
        {
            if (ModelState.IsValid)
            {
                var change = await _username.Change(User, new VerifiedChangeUsername(c.Code, c.NewUsername));
                if (change is Ok<Unit> changeOk)
                {
                    return RedirectToAction("Index", "ProfileSpa", new { returnUrl = c.ReturnUrl });
                }
                else
                {
                    if (change is Error<Unit> resetErr)
                    {
                        _logger.LogError(resetErr.Description);
                    }
                    ModelState.AddModelError("Code", "Not accepted. Please double-check.");

                    return View(v("Token"), c);
                }        
            }
            else
            {
                return View(v("Token"), c);
            }
        }
    }
}