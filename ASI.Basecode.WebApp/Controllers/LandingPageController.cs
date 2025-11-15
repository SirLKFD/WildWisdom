using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace ASI.Basecode.WebApp.Controllers
{
    public class LandingPageController : Controller
    {
        [AllowAnonymous]  // ensures no login required
        public IActionResult Index()
        {
            return View();
        }
    }
}
