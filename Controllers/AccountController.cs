using System.Diagnostics;
using HRwflow.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HRwflow.Controllers
{
    public class AccountController : SessionalController
    {
        private readonly IAuthService _authService;
        private readonly IStorageService<string, CustomerInfo> _customerInfos;
        private readonly IStorageService<string, Customer> _customers;

        public AccountController(IAuthService authService,
            IStorageService<string, CustomerInfo> customerInfos,
            IStorageService<string, Customer> customers) : base(customerInfos, customers)
        {
            _authService = authService;
            _customerInfos = customerInfos;
            _customers = customers;
        }

        [RequireHttps]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Delete()
        {
            if (!TryIdentifyCustomer(out var errorActionResult, loadInfo: false))
            {
                return errorActionResult;
            }
            if (Request.Method.ToUpper() == "GET")
            {
                return View();
            }
            if (Request.Method.ToUpper() == "POST")
            {
                HttpContext.Session.Clear();
                if (!_authService.DeleteAccount(Username).IsCompleted
                    || !_customers.Delete(Username).IsCompleted
                    || !_customerInfos.Delete(Username).IsCompleted)
                {
                    return ShowError(ControllerErrors.OperationFaulted);
                }
                return RedirectAndInform("/", RedirectionModes.Success);
            }
            return ShowError(ControllerErrors.RequestUnsupported);
        }

        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Exit()
        {
            HttpContext.Session.Clear();
            return RedirectAndInform("/account/signin", RedirectionModes.Success);
        }

        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public override IActionResult Main()
        {
            if (!TryIdentifyCustomer(out var errorActionResult))
            {
                return errorActionResult;
            }
            return SelfMain(
                new CustomerVM { Customer = Customer, CustomerInfo = CustomerInfo });
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
            if (!TryIdentifyCustomer(out var errorActionResult, loadInfo: false))
            {
                return errorActionResult;
            }
            var model = new SettingsVM { Customer = Customer };
            if (Request.Method.ToUpper() == "GET")
            {
                return View(model);
            }
            if (Request.Method.ToUpper() == "POST")
            {
                var properties = Customer.Properties;
                model.NameIsCorrect = properties.TrySetName(Request.Form.GetValue("name"));
                if (!properties.Equals(Customer.Properties))
                {
                    Customer.Properties = properties;
                    if (!_customers.Update(Username, Customer).IsCompleted)
                    {
                        return ShowError(ControllerErrors.OperationFaulted);
                    }
                }
                return View(model);
            }
            return ShowError(ControllerErrors.RequestUnsupported);
        }

        [RequireHttps]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult SignIn()
        {
            if (Customer is not null)
            {
                return RedirectMain(RedirectionModes.Success);
            }
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
                var existsResult = _authService.IsUserExists(username);
                if (!existsResult.IsCompleted)
                {
                    return ShowError(ControllerErrors.OperationFaulted);
                }
                if (existsResult.Value)
                {
                    var signInResult = _authService.SignIn(username, password);
                    if (!signInResult.IsCompleted)
                    {
                        return ShowError(ControllerErrors.OperationFaulted);
                    }
                    if (signInResult.Value)
                    {
                        HttpContext.Session.SetString("Username", username);
                        return RedirectMain(RedirectionModes.Success);
                    }
                    model.IsPasswordValid = false;
                }
                else
                {
                    model.IsUserExists = false;
                }
                return View(model);
            }
            return ShowError(ControllerErrors.RequestUnsupported);
        }

        [RequireHttps]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult SignUp()
        {
            if (Customer is not null)
            {
                return RedirectMain(RedirectionModes.Default);
            }
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
                    var result = _authService.IsUserExists(username);
                    if (!result.IsCompleted)
                    {
                        return ShowError(ControllerErrors.OperationFaulted);
                    }
                    model.IsUsernameUnused = !result.Value;
                }
                if (!model.HasErrors)
                {
                    var customer = new Customer { Username = username };
                    var customerInfo = new CustomerInfo { Username = username };
                    if (_customers.Insert(username, customer).IsCompleted
                        && _customerInfos.Insert(username, customerInfo).IsCompleted
                        && _authService.SignUp(username, password).IsCompleted)
                    {
                        HttpContext.Session.SetString("Username", username);
                        return RedirectMain(RedirectionModes.Success);
                    }
                    else
                    {
                        _customers.Delete(username);
                        _customerInfos.Delete(username);
                        return ShowError(ControllerErrors.OperationFaulted);
                    }
                }
                return View(model);
            }
            return ShowError(ControllerErrors.RequestUnsupported);
        }

        private IActionResult RedirectMain(RedirectionModes redirectionMode)
        {
            return RedirectAndInform("/account", redirectionMode);
        }
    }
}
