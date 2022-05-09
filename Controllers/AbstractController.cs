using HRwflow.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;

namespace HRwflow.Controllers
{
    public static class FormExtension
    {
        public static string GetValue(this IFormCollection form, string key)
        {
            if (!form.TryGetValue(key, out StringValues values) || values.Count == 0)
            {
                return null;
            }
            return values[0];
        }
    }

    public abstract class AbstractController : Controller
    {
        public static readonly string ControllerErrorPageName = "ControllerError";
        public static readonly string MainActionName = nameof(Main);
        public static readonly string MainPageName = "Main";
        public static readonly string RedirectionPageName = "Redirection";
        public static readonly string RedirectMainActionName = nameof(RedirectMain);

        public abstract IActionResult Main();

        public abstract IActionResult RedirectMain();

        protected IActionResult RedirectAndInform(string path,
            RedirectionModes redirectionMode = RedirectionModes.Default)
        {
            Response.Redirect(path, permanent: true);
            return View(RedirectionPageName, new RedirectionVM(redirectionMode));
        }

        protected IActionResult SelfMain(object model = null)
        {
            return model == null ? View(MainPageName) : View(MainPageName, model);
        }

        protected IActionResult ShowError(ControllerErrors error = ControllerErrors.Unknown)
        {
            return View(ControllerErrorPageName, new ControllerErrorVM(error));
        }
    }
}
