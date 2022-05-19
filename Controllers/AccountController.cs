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
        private readonly WorkplaceService _workplaceService;

        public AccountController(IAuthService authService,
            IStorageService<string, CustomerInfo> customerInfos,
            IStorageService<string, Customer> customers,
            WorkplaceService workplaceService)
            : base(customerInfos, customers)
        {
            _authService = authService;
            _customerInfos = customerInfos;
            _customers = customers;
            _workplaceService = workplaceService;
        }

        [RequireHttps]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Delete()
        {
            if (!TryIdentifyCustomer(out var errorActionResult, loadInfo: true))
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
                CustomerInfo.AccountState = AccountStates.OnDeletion;
                if (!_customerInfos.Update(Username, CustomerInfo).IsCompleted)
                {
                    return ShowError(ControllerErrors.OperationFaulted);
                }
                var infoResult = _customerInfos.Get(Username);
                if (!infoResult.IsCompleted)
                {
                    return ShowError(ControllerErrors.OperationFaulted);
                }
                foreach (var teamId in infoResult.Value.JoinedTeamNames.Keys)
                {
                    if (_workplaceService.Leave(Username, teamId).HasError)
                    {
                        return ShowError(ControllerErrors.OperationFaulted);
                    }
                }
                if (!_customerInfos.Delete(Username).IsCompleted
                    || !_customers.Delete(Username).IsCompleted
                    || !_authService.DeleteAccount(Username).IsCompleted)
                {
                    return ShowError(ControllerErrors.OperationFaulted);
                }
                return RedirectAndInform("/", RedirectionModes.Success);
            }
            return ShowError(ControllerErrors.RequestUnsupported);
        }

        [HttpGet]
        [RequireHttps]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Exit()
        {
            HttpContext.Session.Clear();
            return RedirectAndInform("/account/signin", RedirectionModes.Success);
        }

        [HttpGet]
        [RequireHttps]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public override IActionResult Main()
        {
            if (!TryIdentifyCustomer(out var errorActionResult, loadInfo: true))
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
            if (!TryIdentifyCustomer(out var errorActionResult))
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
                model.IsNameCorrect = properties.TrySetName(Request.Form.GetValue("name"));
                if (!properties.Equals(Customer.Properties))
                {
                    Customer.Properties = properties;
                    if (!_customers.Update(Username, Customer).IsCompleted)
                    {
                        return ShowError(ControllerErrors.OperationFaulted);
                    }
                }
                var password = Request.Form.GetValue("newPassword");
                model.IsPasswordCorrect = AuthInfo.IsPasswordCorrect(password);
                model.IsPasswordConfirmationCorrect =
                    password == Request.Form.GetValue("passwordConfirmation");
                if (model.IsPasswordCorrect
                    && model.IsPasswordConfirmationCorrect)
                {
                    if (!_authService.UpdatePassword(
                        Username, password).IsCompleted)
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
            if (HttpContext.Session.TryGetValue("Username", out var _))
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

                        if (!Request.Cookies.TryGetValue("RequestedPath",
                            out string path))
                        {
                            path = "/account";
                        }
                        Response.Cookies.Delete("RequestedPath");
                        return RedirectAndInform(path, RedirectionModes.Success);
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
            if (HttpContext.Session.TryGetValue("Username", out var _))
            {
                return RedirectMain(RedirectionModes.Success);
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
                model.IsUsernameCorrect = Customer.IsUsernameCorrect(username);
                model.IsPasswordCorrect = AuthInfo.IsPasswordCorrect(password);
                model.IsPasswordConfirmationCorrect = password == passwordConfirmation;
                if (model.IsUsernameCorrect)
                {
                    var existsResult = _authService.IsUserExists(username);
                    if (!existsResult.IsCompleted)
                    {
                        return ShowError(ControllerErrors.OperationFaulted);
                    }
                    model.IsUsernameUnused = !existsResult.Value;
                }
                if (!model.HasErrors)
                {
                    var customer = new Customer { Username = username };
                    var customerInfo = new CustomerInfo { Username = username };
                    if (_authService.SignUp(username, password).IsCompleted
                        && _customers.Insert(username, customer).IsCompleted
                        && _customerInfos.Insert(username, customerInfo).IsCompleted)
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
