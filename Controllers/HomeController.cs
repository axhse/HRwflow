using Microsoft.AspNetCore.Mvc;

namespace HRwflow.Controllers
{
    public class HomeController : AbstractController
    {
        public HomeController()
        { }

        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return ShowError();
        }

        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public override IActionResult Main()
        {
            return SelfMain();
        }

        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public override IActionResult RedirectMain()
        {
            return RedirectAndInform("/");
        }
    }
}
