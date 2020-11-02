using System;
using System.Threading.Tasks;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;

namespace Ids.Home
{
    public class HomeController : Controller
    {
        readonly IWebHostEnvironment _env;
        readonly IIdentityServerInteractionService _ids4;

        public HomeController(
           IWebHostEnvironment env,
           IIdentityServerInteractionService ids4
        )
        {
            _env = env;
            _ids4 = ids4;
        }

        string v(string name) => $"~/Features/Home/Views/{name}.cshtml";

        public IActionResult TriggerError()
        {
            throw new NotImplementedException();
        }

        public IActionResult Index()
        {
            return View(v("Index"));
        }

        [Route("/error")]
        public async Task<IActionResult> Error(string errorId)
        {
            var e = await _ids4.GetErrorContextAsync(errorId);
            return View(v("Error"), new ErrorDescription { Error = e });
        }
    }
}