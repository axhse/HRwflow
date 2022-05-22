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
            return ShowError();
        }

        [HttpGet]
        public override IActionResult Main()
        {
            return SelfMain();
        }

        [HttpGet]
        public override IActionResult RedirectMain()
        {
            return RedirectAndInform("/");
        }
    }
}
