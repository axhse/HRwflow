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
                return RedirectAndInform($"/workplace/team?teamId={createResult.Value}",
                    RedirectionModes.Success);
            }
            return ShowError(ControllerErrors.RequestUnsupported);
        }

        [RequireHttps]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult EditTeam(int teamId)
        {
            if (!TryIdentifyCustomer(out var errorActionResult))
            {
                return errorActionResult;
            }
            var teamResult = _workplaceService.GetTeam(Username, teamId);
            if (teamResult.HasError)
            {
                return ShowError(teamResult.Error);
            }
            if (!teamResult.Value.Permissions.TryGetValue(Username, out var permissions)
                || !permissions.HasFlag(TeamPermissions.ModifyTeamProperties))
            {
                return ShowError(WorkplaceErrors.NoPermission);
            }
            var model = new EditTeamPropertiesVM
            {
                TeamId = teamId,
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
                       Username, teamId, model.TeamProperties);
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
        public IActionResult Invite(int teamId)
        {
            if (!TryIdentifyCustomer(out var errorActionResult))
            {
                return errorActionResult;
            }
            var teamResult = _workplaceService.GetTeam(Username, teamId);
            if (teamResult.HasError)
            {
                return ShowError(teamResult.Error);
            }
            if (!teamResult.Value.Permissions.TryGetValue(Username, out var permissions)
                || !permissions.HasFlag(TeamPermissions.Invite))
            {
                return ShowError(WorkplaceErrors.NoPermission);
            }
            var model = new IdVM<int>(teamId);
            if (Request.Method.ToUpper() == "GET")
            {
                return View(model);
            }
            if (Request.Method.ToUpper() == "POST")
            {
                string username = Request.Form.GetValue("username");
                var inviteResult = _workplaceService.Invite(Username, teamId, username);
                if (inviteResult.HasError)
                {
                    return ShowError(inviteResult.Error);
                }
                return RedirectAndInform($"/workplace/team?teamId={teamId}",
                    RedirectionModes.Success);
            }
            return ShowError(ControllerErrors.RequestUnsupported);
        }

        [RequireHttps]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult LeaveTeam(int teamId)
        {
            if (!TryIdentifyCustomer(out var errorActionResult))
            {
                return errorActionResult;
            }
            var model = new IdVM<int>(teamId);
            if (Request.Method.ToUpper() == "GET")
            {
                return View(model);
            }
            if (Request.Method.ToUpper() == "POST")
            {
                var leaveResult = _workplaceService.Leave(Username, teamId);
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

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Profile(int teamId, string username)
        {
            if (!TryIdentifyCustomer(out var errorActionResult))
            {
                return errorActionResult;
            }
            var teamResult = _workplaceService.GetTeam(Username, teamId);
            if (teamResult.HasError)
            {
                return ShowError(teamResult.Error);
            }
            if (!teamResult.Value.HasMember(username))
            {
                return ShowError(WorkplaceErrors.UserNotFound);
            }
            var model = new MemberProfileVM(teamResult.Value, Username, username);
            if (Request.Method.ToUpper() == "GET")
            {
                return View(model);
            }
            if (Request.Method.ToUpper() == "POST")
            {
                if (Request.Form.GetValue("kick") is not null)
                {
                    var inviteResult = _workplaceService.Kick(Username, teamId, username);
                    if (inviteResult.HasError)
                    {
                        return ShowError(inviteResult.Error);
                    }
                    return RedirectAndInform($"/workplace/team?teamId={teamId}",
                        RedirectionModes.Success);
                }
                if (!int.TryParse(Request.Form.GetValue("role"), out var roleInt))
                {
                    return ShowError(WorkplaceErrors.ServerError);
                }
                var newRole = (TeamPermissions)roleInt;
                var modifyResult = _workplaceService.ModifyRole(
                            Username, teamId, username, newRole);
                if (modifyResult.HasError)
                {
                    return ShowError(modifyResult.Error);
                }
                model.SubjectPermissions = newRole;
                return View(model);
            }
            return ShowError(ControllerErrors.RequestUnsupported);
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
        public IActionResult Team(int teamId)
        {
            if (!TryIdentifyCustomer(out var errorActionResult))
            {
                return errorActionResult;
            }
            var teamResult = _workplaceService.GetTeam(Username, teamId);
            if (teamResult.HasError)
            {
                return ShowError(teamResult.Error);
            }
            return View(new TeamVM(teamResult.Value, Username));
        }

        [HttpGet]
        [RequireHttps]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult TeamVacancies(int teamId)
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
