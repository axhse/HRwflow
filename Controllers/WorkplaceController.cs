using System;
using System.Collections.Generic;
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
            CheckSession();
            var model = new CreateTeamVM();
            if (Request.Method.ToUpper() == "GET")
            {
                model.CanCreate
                    = CustomerInfo.JoinedTeamNames.Count
                    < WorkplaceLimits.TeamJoinLimit;
                return View(model);
            }
            string name = Request.Form.GetValue("name");
            var properties = new TeamProperties();
            model.IsNameCorrect = properties.TrySetName(name);
            model.Properties = properties;
            if (model.HasErrors)
            {
                return View(model);
            }
            var result = _workplaceService.CreateTeam(
                Username, properties);
            if (result.HasError)
            {
                model.Error = result.Error;
                return View(model);
            }
            return RedirectAndInform("/workplace/team?teamId="
                + $"{result.Value}", RedirectionModes.Success);
        }

        [RequireHttps]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult CreateVacancy(int teamId)
        {
            var teamResult = _workplaceService.GetTeam(
                Username, teamId, TeamPermissions.CreateVacancy);
            if (teamResult.HasError)
            {
                return ShowError(teamResult.Error);
            }
            var team = teamResult.Value;
            var model = new CreateVacancyVM();
            if (Request.Method.ToUpper() == "GET")
            {
                model.CanCreate = team.VacancyCount
                    < WorkplaceLimits.VacanciesMaxCount;
                return View(model);
            }
            string title = Request.Form.GetValue("title");
            var properties = new VacancyProperties();
            model.IsTitleCorrect = properties.TrySetTitle(title);
            model.Properties = properties;
            if (model.HasErrors)
            {
                return View(model);
            }
            var result = _workplaceService.CreateVacancy(
                Username, teamId, properties);
            if (result.HasError)
            {
                model.Error = result.Error;
                return View(model);
            }
            return RedirectAndInform(
                $"/workplace/vacancy?vacancyId={result.Value}",
                RedirectionModes.Success);
        }

        [RequireHttps]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult DeleteVacancy(int vacancyId)
        {
            var vacancyResult
                = _workplaceService.GetVacancy(Username,
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
            _workplaceService.DeleteVacancy(Username, vacancyId);
            return RedirectAndInform("/workplace/vacancies?" +
                $"teamId={vacancyResult.Value.OwnerTeamId}",
                RedirectionModes.Success);
        }

        [RequireHttps]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult EditTeam(int teamId)
        {
            var teamResult
                = _workplaceService.GetTeam(Username, teamId,
                TeamPermissions.ModifyTeamProperties);
            if (teamResult.HasError)
            {
                return ShowError(teamResult.Error);
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
            string name = Request.Form.GetValue("name");
            var properties = model.TeamProperties;
            model.IsNameCorrect = properties.TrySetName(name);
            if (!properties.Equals(model.TeamProperties))
            {
                _workplaceService.ModifyTeamProperties(
                    Username, teamId, properties);
            }
            model.TeamProperties = properties;
            return View(model);
        }

        [RequireHttps]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult EditVacancy(int vacancyId)
        {
            var vacancyResult = _workplaceService.GetVacancy(
                Username, vacancyId,
                TeamPermissions.ModifyVacancy);
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
            var properties = model.VacancyProperties;
            model.IsTitleCorrect = properties.TrySetTitle(
                Request.Form.GetValue("title"));
            model.IsDescriptionCorrect
                = properties.TrySetDescription(
                Request.Form.GetValue("description"));
            if (Enum.TryParse<VacancyStates>(
                Request.Form.GetValue("vacancyState"),
                out var vacancyState))
            {
                properties.State = vacancyState;
            }
            if (!properties.Equals(model.VacancyProperties))
            {
                _workplaceService.ModifyVacancyProperties(
                    Username, vacancyId, properties);
            }
            model.VacancyProperties = properties;
            return View(model);
        }

        [RequireHttps]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Invite(int teamId)
        {
            var teamResult = _workplaceService.GetTeam(
                Username, teamId, TeamPermissions.Invite);
            if (teamResult.HasError)
            {
                return ShowError(teamResult.Error);
            }
            var model = new InviteVM { TeamId = teamId };
            if (Request.Method.ToUpper() == "GET")
            {
                return View(model);
            }
            string username = Request.Form.GetValue("username");
            var result = _workplaceService.Invite(
                Username, teamId, username);
            if (result.HasError)
            {
                model.Error = result.Error;
                return View(model);
            }
            return RedirectAndInform("/workplace/team?teamId=" +
                $"{teamId}", RedirectionModes.Success);
        }

        [RequireHttps]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult LeaveTeam(int teamId)
        {
            CheckSession();
            var model = new IdVM<int>(teamId);
            if (Request.Method.ToUpper() == "GET")
            {
                return View(model);
            }
            _workplaceService.Leave(Username, teamId);

            return RedirectAndInform(
                "/account", RedirectionModes.Success);
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
            var vacancyResult = _workplaceService.GetVacancy(
               Username, vacancyId, TeamPermissions.Observer);
            if (vacancyResult.HasError)
            {
                return ShowError(vacancyResult.Error);
            }
            var vacancy = vacancyResult.Value;
            var teamResult = _workplaceService.GetTeam(Username,
                vacancy.OwnerTeamId, TeamPermissions.Observer);
            if (teamResult.HasError)
            {
                return ShowError(teamResult.Error);
            }
            var permissions
                = teamResult.Value.Permissions[Username];
            var model = new EditVacancyNoteVM
            {
                VacancyId = vacancy.VacancyId,
                CanEdit = Username == username
                    && permissions.HasFlag(
                        TeamPermissions.CommentVacancy)
            };
            model.CanDelete = model.CanEdit
                || permissions.HasFlag(
                    TeamPermissions.ManageVacancyNotes);
            if (!vacancy.Notes.TryGetValue(
                username, out var note))
            {
                if (!model.CanEdit)
                {
                    return ShowError(
                        CommonErrors.ResourceNotFound);
                }
                note = new VacancyNote();
            }
            model.Note = note;
            if (Request.Method.ToUpper() == "GET")
            {
                return View(model);
            }
            var action = Request.Form.GetValue("submit");
            if (action == "delete")
            {
                _workplaceService.DeleteVacancyNote(
                    Username, vacancyId, username);
                return RedirectAndInform(
                    $"/workplace/vacancy?vacancyId={vacancyId}",
                    RedirectionModes.Success);
            }
            var text = Request.Form.GetValue("text");
            model.IsTextCorrect = note.TrySetText(text);
            if (!model.HasErrors)
            {
                _workplaceService.ModifyVacancyNote(
                    Username, vacancyId, model.Note);
            }
            return View(model);
        }

        [RequireHttps]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Profile(
            int teamId, string username)
        {
            var teamResult = _workplaceService.GetTeam(
                Username, teamId, TeamPermissions.Observer);
            if (teamResult.HasError)
            {
                return ShowError(teamResult.Error);
            }
            var model = new MemberProfileVM(
                teamResult.Value, Username, username);
            if (Request.Method.ToUpper() == "GET")
            {
                return View(model);
            }
            if (Request.Form.GetValue("kick") is not null)
            {
                _workplaceService.Kick(
                    Username, teamId, username);
                return RedirectAndInform(
                    $"/workplace/team?teamId={teamId}",
                    RedirectionModes.Success);
            }
            if (!Enum.TryParse<TeamPermissions>(
                Request.Form.GetValue("role"), out var newRole))
            {
                throw new ArgumentException("Invalid form data.");
            }
            _workplaceService.ModifyRole(
                Username, teamId, username, newRole);
            model.SubjectPermissions = newRole;
            return View(model);
        }

        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public override IActionResult RedirectMain()
        {
            return RedirectAndInform("/workplace");
        }

        [HttpGet]
        [RequireHttps]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Team(int teamId)
        {
            var result = _workplaceService.GetTeam(
                Username, teamId, TeamPermissions.Observer);
            if (result.HasError)
            {
                return ShowError(result.Error);
            }
            return View(new TeamVM(result.Value, Username));
        }

        [HttpGet]
        [RequireHttps]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Vacancies(int teamId, string creationTimeOffset,
            string lastNoteTimeOffset, string vacancyStates)
        {
            var teamResult = _workplaceService.GetTeam(
                Username, teamId, TeamPermissions.Observer);
            if (teamResult.HasError)
            {
                return ShowError(teamResult.Error);
            }
            var vacanciesResult
                = _workplaceService.GetVacancies(
                    Username, teamId, TeamPermissions.Observer);
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
            if (Enum.TryParse(creationTimeOffset,
                out TimeSpans timeOffset))
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
                    if (Enum.TryParse(
                        item, out VacancyStates state))
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
            var teamResult = _workplaceService.GetTeam(
                Username, teamId, TeamPermissions.Observer);
            if (teamResult.HasError)
            {
                return ShowError(teamResult.Error);
            }
            var model = new VacancyListVM();
            var creationTimeOffset = model.CreationTimeOffset;
            var lastNoteTimeOffset = model.LastNoteTimeOffset;
            if (Enum.TryParse(Request.Form.GetValue(
                "creationTimeOffset"), out TimeSpans offset))
            {
                creationTimeOffset = offset;
            }
            if (Enum.TryParse(Request.Form.GetValue(
                "lastNoteTimeOffset"), out offset))
            {
                lastNoteTimeOffset = offset;
            }
            HashSet<VacancyStates> selectedStates = new();
            foreach (var vacancyState
                in Enum.GetValues<VacancyStates>())
            {
                if (Request.Form.GetValue(
                    $"vacancyState{vacancyState}") is not null)
                {
                    selectedStates.Add(vacancyState);
                }
            }
            var resultSet = selectedStates.Count > 0
                ? selectedStates : model.VacancyStates;
            var vacancyStates = string.Join(',', resultSet);
            var path = $"/workplace/vacancies?teamId={teamId}"
                + $"&creationTimeOffset={creationTimeOffset}"
                + $"&lastNoteTimeOffset={lastNoteTimeOffset}"
                + $"&vacancyStates={vacancyStates}";
            return RedirectAndInform(path);
        }

        [HttpGet]
        [RequireHttps]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Vacancy(int vacancyId)
        {
            var vacancyResult = _workplaceService.GetVacancy(
                Username, vacancyId, TeamPermissions.Observer);
            if (vacancyResult.HasError)
            {
                return ShowError(vacancyResult.Error);
            }
            var teamResult = _workplaceService.GetTeam(
                Username, vacancyResult.Value.OwnerTeamId,
                TeamPermissions.Observer);
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

        private IActionResult ShowError(CommonErrors error)
        {
            return View("Error",
                new ErrorVM<CommonErrors>(error));
        }
    }
}
