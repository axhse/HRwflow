using System;
using System.Collections.Generic;
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
            if (!TryIdentifyCustomer(out var errorActionResult, loadInfo: true))
            {
                return errorActionResult;
            }
            if (WorkplaceLimits.TeamJoinLimit
                <= CustomerInfo.JoinedTeamNames.Count)
            {
                return ShowError(WorkplaceErrors.JoinLimitExceeded);
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
        public IActionResult CreateVacancy(int teamId)
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
            var team = teamResult.Value;
            if (!team.Permissions.TryGetValue(Username, out var permissions)
                || !permissions.HasFlag(TeamPermissions.CreateVacancy))
            {
                return ShowError(WorkplaceErrors.NoPermission);
            }
            if (WorkplaceLimits.VacanciesMaxCount <= team.VacancyCount)
            {
                return ShowError(WorkplaceErrors.VacancyCountLimitExceeded);
            }
            var model = new EditVacancyPropertiesVM();
            if (Request.Method.ToUpper() == "GET")
            {
                return View(model);
            }
            if (Request.Method.ToUpper() == "POST")
            {
                string title = Request.Form.GetValue("title");
                var vacancyProperties = new VacancyProperties();
                model.VacancyProperties = vacancyProperties;
                model.TitleIsCorrect = vacancyProperties.TrySetTitle(title);
                if (model.HasErrors)
                {
                    return View(model);
                }
                var createResult = _workplaceService.CreateVacancy(
                    Username, teamId, vacancyProperties);
                if (createResult.HasError)
                {
                    return ShowError(createResult.Error);
                }
                return RedirectAndInform($"/workplace/vacancy?vacancyId={createResult.Value}",
                    RedirectionModes.Success);
            }
            return ShowError(ControllerErrors.RequestUnsupported);
        }

        [RequireHttps]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult DeleteVacancy(int vacancyId)
        {
            if (!TryIdentifyCustomer(out var errorActionResult))
            {
                return errorActionResult;
            }
            var vacancyResult = _workplaceService.GetVacancy(Username,
                vacancyId, TeamPermissions.DeleteVacancy);
            if (vacancyResult.HasError)
            {
                return ShowError(vacancyResult.Error);
            }
            var model = new IdVM<int>(vacancyId);
            if (Request.Method.ToUpper() == "GET")
            {
                return View(model);
            }
            if (Request.Method.ToUpper() == "POST")
            {
                var deletionResult = _workplaceService.DeleteVacancy(
                    Username, vacancyId);
                if (deletionResult.HasError)
                {
                    return ShowError(deletionResult.Error);
                }
                return RedirectAndInform(
                    $"/workplace/vacancies?teamId={vacancyResult.Value.OwnerTeamId}",
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
                       Username, teamId, properties);
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
        public IActionResult EditVacancy(int vacancyId)
        {
            if (!TryIdentifyCustomer(out var errorActionResult))
            {
                return errorActionResult;
            }
            var vacancyResult = _workplaceService.GetVacancy(
                Username, vacancyId, TeamPermissions.ModifyVacancy);
            if (vacancyResult.HasError)
            {
                return ShowError(vacancyResult.Error);
            }
            var vacancy = vacancyResult.Value;
            var model = new EditVacancyPropertiesVM
            {
                VacancyId = vacancy.VacancyId,
                VacancyProperties = vacancy.Properties,
            };
            if (Request.Method.ToUpper() == "GET")
            {
                return View(model);
            }
            if (Request.Method.ToUpper() == "POST")
            {
                var properties = model.VacancyProperties;
                model.TitleIsCorrect = properties.TrySetTitle(
                    Request.Form.GetValue("title"));
                model.DescriptionIsCorrect = properties.TrySetDescription(
                    Request.Form.GetValue("description"));
                if (Enum.TryParse<VacancyStates>(Request.Form.GetValue(
                    "vacancyState"), out var vacancyState))
                {
                    properties.State = vacancyState;
                }
                if (!properties.Equals(model.VacancyProperties))
                {
                    model.VacancyProperties = properties;
                    var modifyResult = _workplaceService.ModifyVacancyProperties(
                       Username, vacancyId, properties);
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

        [RequireHttps]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Note(int vacancyId, string username)
        {
            if (!TryIdentifyCustomer(out var errorActionResult))
            {
                return errorActionResult;
            }
            var vacancyResult = _workplaceService.GetVacancy(
               Username, vacancyId);
            if (vacancyResult.HasError)
            {
                return ShowError(vacancyResult.Error);
            }
            var vacancy = vacancyResult.Value;
            var teamResult = _workplaceService.GetTeam(
               Username, vacancy.OwnerTeamId);
            if (teamResult.HasError)
            {
                return ShowError(teamResult.Error);
            }
            var permissions = teamResult.Value.Permissions[Username];
            var model = new EditVacancyNoteVM
            {
                VacancyId = vacancy.VacancyId,
                CanEdit = Username == username
                    && permissions.HasFlag(TeamPermissions.CommentVacancy),
                CanDelete = permissions.HasFlag(TeamPermissions.ManageVacancyNotes)
            };
            if (!vacancy.Notes.TryGetValue(username, out var note))
            {
                if (!model.CanEdit)
                {
                    return ShowError(WorkplaceErrors.ResourceNotFound);
                }
                note = new VacancyNote();
            }
            model.Note = note;
            if (Request.Method.ToUpper() == "GET")
            {
                return View(model);
            }
            if (Request.Method.ToUpper() == "POST")
            {
                var action = Request.Form.GetValue("submit");
                if (action == "delete")
                {
                    var deletionResult = _workplaceService.DeleteVacancyNote(
                        Username, vacancyId, username);
                    if (deletionResult.HasError)
                    {
                        return ShowError(deletionResult.Error);
                    }
                    return RedirectAndInform(
                        $"/workplace/vacancy?vacancyId={vacancyId}",
                        RedirectionModes.Success);
                }
                var text = Request.Form.GetValue("text");
                model.TextIsCorrect = note.TrySetText(text);
                if (!model.HasErrors)
                {
                    var updateResult = _workplaceService.ModifyVacancyNote(
                        Username, vacancyId, model.Note);
                    if (updateResult.HasError)
                    {
                        return ShowError(updateResult.Error);
                    }
                }
                return View(model);
            }
            return ShowError(ControllerErrors.RequestUnsupported);
        }

        [RequireHttps]
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
                if (!Enum.TryParse<TeamPermissions>(
                    Request.Form.GetValue("role"), out var newRole))
                {
                    return ShowError(WorkplaceErrors.ServerError);
                }
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
        public IActionResult Vacancies(int teamId, string creationTimeOffset,
            string lastNoteTimeOffset, string vacancyStates)
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
            var vacanciesResult = _workplaceService.GetVacancies(Username, teamId);
            if (vacanciesResult.HasError)
            {
                return ShowError(vacanciesResult.Error);
            }
            var model = new VacancyListVM
            {
                AllVacancies = vacanciesResult.Value,
                TeamId = teamResult.Value.TeamId,
                TeamName = teamResult.Value.Properties.Name,
            };
            if (Enum.TryParse(creationTimeOffset, out TimeSpans timeOffset))
            {
                model.CreationTimeOffset = timeOffset;
            }
            if (Enum.TryParse(lastNoteTimeOffset, out timeOffset))
            {
                model.LastNoteTimeOffset = timeOffset;
            }
            if (vacancyStates is not null)
            {
                HashSet<VacancyStates> selectedStates = new();
                foreach (var item in vacancyStates.Split(','))
                {
                    if (Enum.TryParse(item, out VacancyStates state))
                    {
                        selectedStates.Add(state);
                    }
                }
                if (selectedStates.Count > 0)
                {
                    model.VacancyStates = selectedStates;
                }
            }
            return View(model);
        }

        [HttpPost]
        [RequireHttps]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Vacancies(int teamId)
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
            var model = new VacancyListVM();
            var creationTimeOffset = model.CreationTimeOffset;
            var lastNoteTimeOffset = model.LastNoteTimeOffset;
            if (Enum.TryParse(Request.Form.GetValue("creationTimeOffset"),
                out TimeSpans offset))
            {
                creationTimeOffset = offset;
            }
            if (Enum.TryParse(
                Request.Form.GetValue("lastNoteTimeOffset"), out offset))
            {
                lastNoteTimeOffset = offset;
            }
            HashSet<VacancyStates> selectedStates = new();
            foreach (var vacancyState in Enum.GetValues<VacancyStates>())
            {
                if (Request.Form.GetValue($"vacancyState{vacancyState}")
                    is not null)
                {
                    selectedStates.Add(vacancyState);
                }
            }
            var resultSet = selectedStates.Count > 0
                ? selectedStates : model.VacancyStates;
            var vacancyStates = string.Join(',', resultSet);
            var path = $"/workplace/vacancies?teamId={teamId}" +
                $"&creationTimeOffset={creationTimeOffset}" +
                $"&lastNoteTimeOffset={lastNoteTimeOffset}" +
                $"&vacancyStates={vacancyStates}";
            return RedirectAndInform(path);
        }

        [HttpGet]
        [RequireHttps]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Vacancy(int vacancyId)
        {
            if (!TryIdentifyCustomer(out var errorActionResult))
            {
                return errorActionResult;
            }
            var vacancyResult = _workplaceService.GetVacancy(Username, vacancyId);
            if (vacancyResult.HasError)
            {
                return ShowError(vacancyResult.Error);
            }
            var teamResult = _workplaceService.GetTeam(
                Username, vacancyResult.Value.OwnerTeamId);
            if (teamResult.HasError)
            {
                return ShowError(teamResult.Error);
            }
            var model = new VacancyVM
            {
                Team = teamResult.Value,
                Vacancy = vacancyResult.Value,
                Username = Username
            };
            return View(model);
        }

        private IActionResult ShowError(WorkplaceErrors error)
        {
            return View(ErrorPageName, new WorkplaceErrorVM(error));
        }
    }
}
