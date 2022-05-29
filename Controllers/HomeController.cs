using Microsoft.AspNetCore.Mvc;

namespace HRwflow.Controllers
{
    public class HomeController : AbstractController
    {
        public HomeController()
        { }

        [HttpGet]
        public IActionResult Error()
        {
            return View();
        }

        [HttpGet]
        public override IActionResult Main()
        {
            return View();
        }

        [HttpGet]
        public override IActionResult RedirectMain()
        {
            return RedirectAndInform("/");
        }
    }
}
