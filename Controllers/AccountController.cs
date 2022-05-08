using System;
using System.Diagnostics;
using System.Text;
using HRwflow.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HRwflow.Controllers
{
    public class AccountController : AbstractController
    {
        private readonly IAuthService _authService;
        private readonly IStorageService<string, CustomerInfo> _customerInfo;
        private readonly IStorageService<string, Customer> _customers;

        public AccountController(IAuthService authService,
            IStorageService<string, Customer> customers,
            IStorageService<string, CustomerInfo> customerInfo)
        {
            _authService = authService;
            _customers = customers;
            _customerInfo = customerInfo;
        }

        [RequireHttps]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Delete()
        {
            string username = HttpContext.Session.GetString("Username");
            var result = _customers.Get(username).Result;
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
                if (!_customers.Delete(username).Result.IsSuccessful)
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
            var result = _customers.Get(username).Result;
            var infoResult = _customerInfo.Get(username).Result;
            if (result.IsCompleted && !result.IsSuccessful)
            {
                return RedirectSignIn();
            }
            if (!result.IsCompleted || !infoResult.IsSuccessful)
            {
                return ShowError(ErrorTypes.OperationFaulted);
            }
            return SelfMain(
                new CustomerVM { Customer = result.Value, CustomerInfo = infoResult.Value });
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
            var result = _customers.Get(username).Result;
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
                var properties = customer.Properties;
                try
                {
                    properties.Name = Request.Form.GetValue("name");
                }
                catch (ArgumentException)
                {
                    model.NameIsCorrect = false;
                }
                if (!properties.Equals(customer.Properties))
                {
                    customer.Properties = properties;
                    if (!_customers.Update(username, customer).Result.IsSuccessful)
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

                var result = _customers.Get(username).Result;
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
                    var result = _customers.Get(username).Result;
                    if (!result.IsCompleted)
                    {
                        return ShowError(ErrorTypes.OperationFaulted);
                    }
                    model.IsUsernameUnused = !result.IsSuccessful;
                }
                if (!model.HasErrors)
                {
                    var customer = new Customer { Username = username };
                    var result = _customers.Insert(customer).Result;
                    if (!result.IsCompleted)
                    {
                        return ShowError(ErrorTypes.OperationFaulted);
                    }
                    if (!result.IsSuccessful)
                    {
                        model.IsUsernameUnused = false;
                    }
                    else
                    {
                        var customerInfo = new CustomerInfo { Username = username };
                        if (!_customerInfo.Insert(customerInfo).Result.IsSuccessful
                            || !_authService.SignUp(username, password).Result.IsSuccessful)
                        {
                            _customers.Delete(username);
                            _customerInfo.Delete(username);
                            return ShowError(ErrorTypes.OperationFaulted);
                        }
                        else
                        {
                            HttpContext.Session.SetString("Username", username);
                            return RedirectMain(RedirectionModes.Success);
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
