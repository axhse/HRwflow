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
        public abstract IActionResult Main();

        public abstract IActionResult RedirectMain();

        protected IActionResult RedirectAndInform(string path,
            RedirectionModes redirectionMode = RedirectionModes.Default)
        {
            Response.Redirect(path, permanent: true);
            return View("Redirection", new RedirectionVM(redirectionMode));
        }
    }
}
