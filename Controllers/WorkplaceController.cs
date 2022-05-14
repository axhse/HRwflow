using System.Diagnostics;
using HRwflow.Models;
using Microsoft.AspNetCore.Mvc;

namespace HRwflow.Controllers
{
    public class WorkplaceController : SessionalController
    {
        public static readonly string ErrorPageName = "Error";
        private readonly WorkplaceService _workplaceService;

        public WorkplaceController(IStorageService<string, CustomerInfo> customerInfos,
            IStorageService<string, Customer> customers,
            WorkplaceService workplaceService) : base(customerInfos, customers)
        {
            _workplaceService = workplaceService;
        }

        [RequireHttps]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult CreateTeam()
        {
            if (!TryIdentifyCustomer(out var errorActionResult))
            {
                return errorActionResult;
            }
            var model = new EditTeamPropertiesVM();
            if (Request.Method.ToUpper() == "GET")
            {
                return View(model);
            }
            if (Request.Method.ToUpper() == "POST")
            {
                string name = Request.Form.GetValue("name");
                var teamProperties = new TeamProperties();
                model.TeamProperties = teamProperties;
                model.NameIsCorrect = teamProperties.TrySetName(name);
                if (model.HasErrors)
                {
                    return View(model);
                }
                var createResult = _workplaceService.CreateTeam(Username, teamProperties);
                if (createResult.HasError)
                {
                    return ShowError(createResult.Error);
                }
                return RedirectAndInform($"/workplace/team/{createResult.Value}",
                    RedirectionModes.Success);
            }
            return ShowError(ControllerErrors.RequestUnsupported);
        }

        [RequireHttps]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult EditTeam(int id)
        {
            if (!TryIdentifyCustomer(out var errorActionResult))
            {
                return errorActionResult;
            }
            var teamResult = _workplaceService.GetTeam(Username, id);
            if (teamResult.HasError)
            {
                return ShowError(teamResult.Error);
            }
            var model = new EditTeamPropertiesVM
            {
                TeamId = id,
                TeamProperties = teamResult.Value.Properties,
            };
            if (Request.Method.ToUpper() == "GET")
            {
                return View(model);
            }
            if (Request.Method.ToUpper() == "POST")
            {
                string name = Request.Form.GetValue("name");
                var properties = model.TeamProperties;
                model.NameIsCorrect = properties.TrySetName(name);
                if (!properties.Equals(model.TeamProperties))
                {
                    model.TeamProperties = properties;
                    var modifyResult = _workplaceService.ModifyTeamProperties(
                       Username, id, model.TeamProperties);
                    if (modifyResult.HasError)
                    {
                        return ShowError(modifyResult.Error);
                    }
                }
                return View(model);
            }
            return ShowError(ControllerErrors.RequestUnsupported);
        }

        [RequireHttps]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Invite(int id)
        {
            if (!TryIdentifyCustomer(out var errorActionResult))
            {
                return errorActionResult;
            }
            var model = new IdVM<int>(id);
            if (Request.Method.ToUpper() == "GET")
            {
                return View(model);
            }
            if (Request.Method.ToUpper() == "POST")
            {
                string username = Request.Form.GetValue("username");
                var inviteResult = _workplaceService.Invite(Username, id, username);
                if (inviteResult.HasError)
                {
                    return ShowError(inviteResult.Error);
                }
                return RedirectAndInform($"/workplace/team/{id}",
                    RedirectionModes.Success);
            }
            return ShowError(ControllerErrors.RequestUnsupported);
        }

        [RequireHttps]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Kick(int id)
        {
            if (!TryIdentifyCustomer(out var errorActionResult))
            {
                return errorActionResult;
            }
            var model = new IdVM<int>(id);
            if (Request.Method.ToUpper() == "GET")
            {
                return View(model);
            }
            if (Request.Method.ToUpper() == "POST")
            {
                string username = Request.Form.GetValue("username");
                var inviteResult = _workplaceService.Kick(Username, id, username);
                if (inviteResult.HasError)
                {
                    return ShowError(inviteResult.Error);
                }
                return RedirectAndInform($"/workplace/team/{id}",
                    RedirectionModes.Success);
            }
            return ShowError(ControllerErrors.RequestUnsupported);
        }

        [RequireHttps]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult LeaveTeam(int id)
        {
            if (!TryIdentifyCustomer(out var errorActionResult))
            {
                return errorActionResult;
            }
            var model = new IdVM<int>(id);
            if (Request.Method.ToUpper() == "GET")
            {
                return View(model);
            }
            if (Request.Method.ToUpper() == "POST")
            {
                var leaveResult = _workplaceService.Leave(Username, id);
                if (leaveResult.HasError)
                {
                    return ShowError(leaveResult.Error);
                }
                return RedirectAndInform("/account", RedirectionModes.Success);
            }
            return ShowError(ControllerErrors.RequestUnsupported);
        }

        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public override IActionResult Main()
        {
            return RedirectAndInform("/account");
        }

        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public override IActionResult RedirectMain()
        {
            return Main();
        }

        [HttpGet]
        [RequireHttps]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Team(int id)
        {
            if (!TryIdentifyCustomer(out var errorActionResult))
            {
                return errorActionResult;
            }
            var teamResult = _workplaceService.GetTeam(Username, id);
            if (teamResult.HasError)
            {
                return ShowError(teamResult.Error);
            }
            return View(new TeamVM { Team = teamResult.Value });
        }

        [HttpGet]
        [RequireHttps]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult TeamVacancies(int id)
        {
            if (!TryIdentifyCustomer(out var errorActionResult))
            {
                return errorActionResult;
            }
            // TODO
            return ShowError(ControllerErrors.RequestUnsupported);
        }

        private IActionResult ShowError(WorkplaceErrors error)
        {
            return View(ErrorPageName, new WorkplaceErrorVM(error));
        }
    }
}
