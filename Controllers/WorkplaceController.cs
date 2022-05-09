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
            if (!TryIdentifyCustomer(out var errorActionResult, loadInfo: false))
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
                return RedirectAndInform($"/workplace/editteam/{createResult.Value}",
                    RedirectionModes.Success);
            }
            return ShowError(ControllerErrors.RequestUnsupported);
        }

        [RequireHttps]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult EditTeam(int id)
        {
            if (!TryIdentifyCustomer(out var errorActionResult, loadInfo: false))
            {
                return errorActionResult;
            }
            var infoResult = _workplaceService.GetTeamInfo(Username, id);
            if (infoResult.HasError)
            {
                return ShowError(infoResult.Error);
            }
            var info = infoResult.Value;
            var model = new EditTeamPropertiesVM { TeamProperties = info.Properties };
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
        public override IActionResult Main()
        {
            return RedirectAndInform("/account");
        }

        [RequireHttps]
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
            if (!TryIdentifyCustomer(out var errorActionResult, loadInfo: false))
            {
                return errorActionResult;
            }
            var result = _workplaceService.GetTeamInfo(Username, id);
            if (result.HasError)
            {
                return ShowError(result.Error);
            }
            return View(new TeamInfoVM { TeamInfo = result.Value });
        }

        [HttpGet]
        [RequireHttps]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult TeamVacancies(int id)
        {
            if (!TryIdentifyCustomer(out var errorActionResult, loadInfo: false))
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
