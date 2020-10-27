using Microsoft.AspNetCore.Mvc;

namespace Ids.Home
{
    public class HomeController : Controller
    {
        string v(string name) => $"~/Features/Home/Views/{name}.cshtml";

        public IActionResult Index()
        {
            return View(v("Index"));
        }

        public IActionResult Error()
        {
            return View(v("Error"));
        }
    }
}