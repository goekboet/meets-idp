using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ids.Profile
{
    [Authorize]
    public class ProfileSpaController : Controller
    {
        public string v(string n) => $"~/Features/Profile/{n}.cshtml";

        [Route("/profile")]
        [HttpGet, HttpHead]
        public IActionResult Index()
        {
            return View(v("Index"));
        }
    }
}