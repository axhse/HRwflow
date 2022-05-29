using System;
using HRwflow.Models;
using Microsoft.AspNetCore.Http;

namespace HRwflow.Controllers
{
    public abstract class SessionalController : AbstractController
    {
        private readonly IStorageService<string, CustomerInfo> _customerInfos;
        private readonly IStorageService<string, Customer> _customers;
        private Customer _customer;
        private CustomerInfo _customerInfo;
        private bool _isCustomerInit = false;
        private bool _isCustomerInfoInit = false;
        private readonly object _initSyncRoot = new();

        public SessionalController(
            IStorageService<string, CustomerInfo> customerInfos,
            IStorageService<string, Customer> customers)
        {
            _customerInfos = customerInfos;
            _customers = customers;
        }

        protected Customer Customer
        {
            get
            {
                lock (_initSyncRoot)
                {
                    if (!_isCustomerInit)
                    {
                        InitCustomer();
                        _isCustomerInit = true;
                    }
                }
                return _customer;
            }
        }

        protected CustomerInfo CustomerInfo
        {
            get
            {
                lock (_initSyncRoot)
                {
                    if (!_isCustomerInit)
                    {
                        InitCustomer();
                        _isCustomerInit = true;
                    }
                    if (!_isCustomerInfoInit)
                    {
                        InitCustomerInfo();
                        _isCustomerInfoInit = true;
                    }
                }
                return _customerInfo;
            }
        }

        protected void CheckSession()
        {
            lock (_initSyncRoot)
            {
                if (!_isCustomerInit)
                {
                    InitCustomer();
                    _isCustomerInit = true;
                }
            }
        }

        private void InitCustomer()
        {
            string username
                = HttpContext.Session.GetString("Username");
            if (username is null
                || !_customers.TryFind(username, out _customer))
            {
                HttpContext.Session.Clear();
                if (Request.Path.HasValue)
                {
                    var path = Request.Path.Value;
                    if (Request.QueryString.HasValue)
                    {
                        path += Request.QueryString.Value;
                    }
                    Response.Cookies.Append(
                        "RequestedPath", path);
                }
                Response.Redirect("/account/signin", true);
                Response.CompleteAsync().Wait();
                throw new InvalidOperationException(
                    "Session is empty" +
                    " or Customer data not found.");
            }
        }

        private void InitCustomerInfo()
        {
            if (!_customerInfos.TryFind(
                Username, out _customerInfo))
            {
                HttpContext.Session.Clear();
                Response.Redirect("/home/error", true);
                Response.CompleteAsync().Wait();
                throw new InvalidOperationException(
                    "CustomerInfo data not found.");
            }
        }

        protected string Username => Customer.Username;
    }
}
