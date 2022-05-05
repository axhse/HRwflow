using System.Diagnostics;
using System.Text;
using HRwflow.Models;
using HRwflow.Models.Data;
using HRwflow.Models.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HRwflow.Controllers
{
    public class AccountController : AbstractController
    {
        private readonly IAuthService _authService;
        private readonly IStorageService<string, Customer> _customerService;

        public AccountController(AuthDataDbContext authDataDbContext,
            CustomerDbContext customerDbContext)
        {
            _authService = new AuthService(new DbContextService<string, AuthData>(authDataDbContext));
            _customerService = new DbContextService<string, Customer>(customerDbContext);
        }

        [RequireHttps]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Delete()
        {
            string username = HttpContext.Session.GetString("Username");
            var result = _customerService.Get(username).Result;
            if (!result.IsCompleted)
            {
                return ShowError(ErrorTypes.OperationFaulted);
            }
            if (!result.IsSuccessful)
            {
                return RedirectSignIn();
            }
            if (Request.Method.ToUpper() == "GET")
            {
                return View();
            }
            if (Request.Method.ToUpper() == "POST")
            {
                if (!_customerService.Delete(username).Result.IsSuccessful)
                {
                    return ShowError(ErrorTypes.OperationFaulted);
                }
                if (!_authService.Delete(username).Result.IsSuccessful)
                {
                    return ShowError(ErrorTypes.OperationFaulted);
                }
                HttpContext.Session.Clear();
                return RedirectAndInform("/", RedirectionModes.Success);
            }
            return ShowError(ErrorTypes.RequestUnsupported);
        }

        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Exit()
        {
            HttpContext.Session.Clear();
            return RedirectSignIn(RedirectionModes.Success);
        }

        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public override IActionResult Main()
        {
            string username = HttpContext.Session.GetString("Username");
            var result = _customerService.Get(username).Result;
            if (!result.IsCompleted)
            {
                return ShowError(ErrorTypes.OperationFaulted);
            }
            if (!result.IsSuccessful)
            {
                return RedirectSignIn();
            }
            return SelfMain(new CustomerVM { Customer = result.Value });
        }

        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public override IActionResult RedirectMain()
        {
            return RedirectMain(RedirectionModes.Default);
        }

        [RequireHttps]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Settings()
        {
            string username = HttpContext.Session.GetString("Username");
            var result = _customerService.Get(username).Result;
            if (!result.IsCompleted)
            {
                return ShowError(ErrorTypes.OperationFaulted);
            }
            if (!result.IsSuccessful)
            {
                return RedirectSignIn();
            }
            var customer = result.Value;
            var model = new SettingsVM { Customer = customer };
            if (Request.Method.ToUpper() == "GET")
            {
                return View(model);
            }
            if (Request.Method.ToUpper() == "POST")
            {
                string name = CustomerProperties.FormatName(Request.Form.GetValue("name"));
                bool updateRequired = false;
                if (CustomerProperties.NameIsCorrect(name))
                {
                    if (name != customer.Properties.Name)
                    {
                        customer.Properties.Name = name;
                        updateRequired = true;
                    }
                }
                else
                {
                    model.NameIsCorrect = false;
                }
                if (updateRequired)
                {
                    if (!_customerService.Update(username, customer).Result.IsSuccessful)
                    {
                        return ShowError(ErrorTypes.OperationFaulted);
                    }
                }
                return View(model);
            }
            return ShowError(ErrorTypes.RequestUnsupported);
        }

        [RequireHttps]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult SignIn()
        {
            var model = new SignInVM();
            if (Request.Method.ToUpper() == "GET")
            {
                if (!Request.Cookies.TryGetValue("Username", out string username))
                {
                    model.RememberMeChecked = false;
                }
                model.DefaultUsername = username;
                return View(model);
            }
            if (Request.Method.ToUpper() == "POST")
            {
                string username = Request.Form.GetValue("username");
                string password = Request.Form.GetValue("password");
                bool rememberMeChecked = Request.Form.GetValue("rememberMe") is not null;
                username = Customer.FormatUsername(username);
                model.DefaultUsername = username;
                model.RememberMeChecked = rememberMeChecked;
                if (rememberMeChecked)
                {
                    Response.Cookies.Append("Username", username);
                }
                else
                {
                    Response.Cookies.Delete("Username");
                }

                var result = _customerService.Get(username).Result;
                if (!result.IsCompleted)
                {
                    return ShowError(ErrorTypes.OperationFaulted);
                }
                if (result.IsSuccessful)
                {
                    var signInResult = _authService.SignIn(username, password).Result;
                    if (!signInResult.IsCompleted)
                    {
                        return ShowError(ErrorTypes.OperationFaulted);
                    }
                    model.IsPasswordValid = signInResult.IsSuccessful;
                    if (!model.HasErrors)
                    {
                        HttpContext.Session.SetString("Username", username);
                        return RedirectMain(RedirectionModes.Success);
                    }
                }
                else
                {
                    model.IsUserExists = false;
                }
                return View(model);
            }
            return ShowError(ErrorTypes.RequestUnsupported);
        }

        [RequireHttps]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult SignUp()
        {
            var model = new SignUpVM();
            if (Request.Method.ToUpper() == "GET")
            {
                return View(model);
            }
            if (Request.Method.ToUpper() == "POST")
            {
                string username = Request.Form.GetValue("username");
                string password = Request.Form.GetValue("password");
                string passwordConfirmation = Request.Form.GetValue("passwordConfirmation");
                username = Customer.FormatUsername(username);
                model.DefaultUsername = username;
                model.IsUsernameCorrect = Customer.UsernameIsCorrect(username);
                model.IsPasswordCorrect = Customer.PasswordIsCorrect(password);
                model.IsPasswordConfirmationCorrect = password == passwordConfirmation;
                if (model.IsUsernameCorrect)
                {
                    var result = _customerService.Get(username).Result;
                    if (!result.IsCompleted)
                    {
                        return ShowError(ErrorTypes.OperationFaulted);
                    }
                    model.IsUsernameUnused = !result.IsSuccessful;
                }
                if (!model.HasErrors)
                {
                    var result = _authService.SignUp(username, password).Result;
                    if (!result.IsCompleted)
                    {
                        return ShowError(ErrorTypes.OperationFaulted);
                    }
                    if (result.IsSuccessful)
                    {
                        var customer = new Customer { Username = username };
                        var insertResult = _customerService.Insert(customer).Result;
                        if (!insertResult.IsCompleted)
                        {
                            return ShowError(ErrorTypes.OperationFaulted);
                        }
                        if (insertResult.IsSuccessful)
                        {
                            HttpContext.Session.SetString("Username", username);
                            return RedirectMain(RedirectionModes.Success);
                        }
                        else
                        {
                            model.IsUsernameUnused = false;
                        }
                    }
                }
                return View(model);
            }
            return ShowError(ErrorTypes.RequestUnsupported);
        }

        private IActionResult RedirectMain(RedirectionModes redirectionMode)
        {
            return RedirectAndInform("/account", redirectionMode);
        }

        private IActionResult RedirectSignIn(
            RedirectionModes redirectionMode = RedirectionModes.Warning)
        {
            return RedirectAndInform("/account/signin", redirectionMode);
        }
    }
}
