using System.Threading.Tasks;
using Ids.AspIdentity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Ids.Logout
{
    public class LogoutController : Controller
    {
        private readonly ILogger<LogoutController> _logger;
        private readonly SignInManager<IdsUser> _session;

        public LogoutController(
            ILogger<LogoutController> logger,
            SignInManager<IdsUser> session
        )
        {
            _logger = logger;
            _session = session;
        }

        string v(string name) => $"~/Features/Logout/Views/{name}.cshtml";

        [HttpGet, HttpHead]
        public IActionResult Index()
        {
            return View(v("Index"), new LogoutInput());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EndSession(
            LogoutInput i
        )
        {
            if (ModelState.IsValid)
            {
                await _session.SignOutAsync();
                return RedirectToAction("SessionEnd", "Logout", new { id = i.LogoutId });
            }
            else
            {
                _logger.LogWarning("Malformed input to EndSession.");
                return View(v("Index"), new LogoutInput());
            }
        }

        [HttpGet, HttpHead]
        public IActionResult SessionEnd(string id)
        {
            return View(v("SessionEnd"));
        }
    }
}