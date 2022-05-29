using HRwflow.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HRwflow.Controllers
{
    public class AccountController : SessionalController
    {
        private readonly AuthService _authService;
        private readonly IStorageService<string, CustomerInfo> _customerInfos;
        private readonly IStorageService<string, Customer> _customers;
        private readonly WorkplaceService _workplaceService;

        public AccountController(
            IStorageService<string, CustomerInfo> customerInfos,
            IStorageService<string, Customer> customers,
            AuthService authService,
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
            CheckSession();
            if (Request.Method.ToUpper() == "GET")
            {
                return View();
            }
            CustomerInfo.AccountState
                = AccountStates.OnDeletion;
            _customerInfos.Update(Username, CustomerInfo);
            var info = _customerInfos.Find(Username);
            foreach (var teamId in info.JoinedTeamNames.Keys)
            {
                _workplaceService.Leave(Username, teamId);
            }
            _customerInfos.Delete(Username);
            _customers.Delete(Username);
            _authService.DeleteAccount(Username);
            HttpContext.Session.Clear();
            return RedirectAndInform("/",
                RedirectionModes.Success);
        }

        [HttpGet]
        [RequireHttps]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Exit()
        {
            HttpContext.Session.Clear();
            return RedirectAndInform("/account/signin",
                RedirectionModes.Success);
        }

        [HttpGet]
        [RequireHttps]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public override IActionResult Main()
        {
            return View(new CustomerVM
            {
                Customer = Customer,
                CustomerInfo = CustomerInfo
            });
        }

        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public override IActionResult RedirectMain()
        {
            return RedirectMain();
        }

        [RequireHttps]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Settings()
        {
            var model = new SettingsVM { Customer = Customer };
            if (Request.Method.ToUpper() == "GET")
            {
                return View(model);
            }
            var properties = Customer.Properties;
            model.IsNameCorrect = properties.TrySetName(
                Request.Form.GetValue("name"));
            if (!properties.Equals(Customer.Properties))
            {
                Customer.Properties = properties;
                _customers.Update(Username, Customer);
            }
            var password = Request.Form.GetValue("newPassword");
            if (password is not null
                && password != string.Empty)
            {
                model.IsPasswordCorrect
                    = AuthInfo.IsPasswordCorrect(password);
                model.IsPasswordConfirmationCorrect =
                    password == Request.Form.GetValue(
                        "passwordConfirmation");
                if (model.IsPasswordCorrect
                    && model.IsPasswordConfirmationCorrect)
                {
                    _authService.UpdatePassword(
                        Username, password);
                }
            }
            return View(model);
        }

        [RequireHttps]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult SignIn()
        {
            if (HttpContext.Session.TryGetValue(
                "Username", out _))
            {
                return RedirectMain(RedirectionModes.Success);
            }
            var model = new SignInVM();
            if (Request.Method.ToUpper() == "GET")
            {
                if (!Request.Cookies.TryGetValue(
                    "Username", out string rememberedUsername))
                {
                    model.IsRememberMeChecked = false;
                }
                model.DefaultUsername = rememberedUsername;
                return View(model);
            }
            string username = Request.Form.GetValue("username");
            string password = Request.Form.GetValue("password");
            bool rememberMeChecked = Request.Form.GetValue(
                "rememberMe") is not null;
            username = Customer.FormatUsername(username);
            model.DefaultUsername = username;
            model.IsRememberMeChecked = rememberMeChecked;
            if (rememberMeChecked)
            {
                Response.Cookies.Append("Username", username);
            }
            else
            {
                Response.Cookies.Delete("Username");
            }
            var result = _authService.SignIn(
                username, password);
            if (result.HasError)
            {
                model.Error = result.Error;
                return View(model);
            }
            HttpContext.Session.SetString("Username", username);
            if (!Request.Cookies.TryGetValue("RequestedPath",
                out string path))
            {
                path = "/account";
            }
            Response.Cookies.Delete("RequestedPath");
            return RedirectAndInform(path,
                RedirectionModes.Success);
        }

        [RequireHttps]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult SignUp()
        {
            if (HttpContext.Session.TryGetValue(
                "Username", out _))
            {
                return RedirectMain(RedirectionModes.Success);
            }
            var model = new SignUpVM();
            if (Request.Method.ToUpper() == "GET")
            {
                return View(model);
            }
            string username = Request.Form.GetValue("username");
            string password = Request.Form.GetValue("password");
            string passwordConfirmation
                = Request.Form.GetValue("passwordConfirmation");
            username = Customer.FormatUsername(username);
            model.DefaultUsername = username;
            model.IsUsernameCorrect
                = Customer.IsUsernameCorrect(username);
            model.IsPasswordCorrect
                = AuthInfo.IsPasswordCorrect(password);
            model.IsPasswordConfirmationCorrect
                = password == passwordConfirmation;
            if (model.IsUsernameCorrect)
            {
                model.IsUsernameUnused
                    = !_authService.IsAccountExists(username);
            }
            if (model.HasErrors)
            {
                return View(model);
            }
            var customer = new Customer { Username = username };
            var customerInfo
                = new CustomerInfo { Username = username };
            var result
                = _authService.SignUp(username, password);
            if (result.HasError)
            {
                model.Error = result.Error;
                return View(model);
            }
            _customers.Insert(username, customer);
            _customerInfos.Insert(username, customerInfo);
            HttpContext.Session.SetString("Username", username);
            return RedirectMain(RedirectionModes.Success);
        }

        private IActionResult RedirectMain(
            RedirectionModes redirectionMode
            = RedirectionModes.Default)
        {
            return RedirectAndInform("/account", redirectionMode);
        }
    }
}
