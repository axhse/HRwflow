using HRwflow.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HRwflow.Controllers
{
    public abstract class SessionalController : AbstractController
    {
        private readonly IStorageService<string, CustomerInfo> _customerInfos;
        private readonly IStorageService<string, Customer> _customers;

        public SessionalController(
            IStorageService<string, CustomerInfo> customerInfos,
            IStorageService<string, Customer> customers)
        {
            _customerInfos = customerInfos;
            _customers = customers;
        }

        protected Customer Customer { get; set; }
        protected CustomerInfo CustomerInfo { get; set; }
        protected string Username => Customer.Username;

        protected bool TryIdentifyCustomer(
            out IActionResult errorActionResult, bool loadInfo = false)
        {
            string username = HttpContext.Session.GetString("Username");
            if (username is null)
            {
                if (Request.Path.HasValue)
                {
                    var path = Request.Path.Value;
                    if (Request.QueryString.HasValue)
                    {
                        path += Request.QueryString.Value;
                    }
                    Response.Cookies.Append("RequestedPath", path);
                }
                errorActionResult
                    = RedirectAndInform("/account/signin", RedirectionModes.Warning);
                return false;
            }
            var customerResult = _customers.Get(username);
            var infoResult = loadInfo ? _customerInfos.Get(username) : null;
            if (!customerResult.IsCompleted
                || (infoResult is not null && !infoResult.IsCompleted))
            {
                HttpContext.Session.Clear();
                errorActionResult = ShowError(ControllerErrors.OperationFaulted);
                return false;
            }
            errorActionResult = null;
            Customer = customerResult.Value;
            CustomerInfo = loadInfo ? infoResult.Value : null;
            return true;
        }
    }
}
