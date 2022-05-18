using System.Collections.Generic;
using System.Linq;

namespace HRwflow.Models
{
    public static class WorkplaceLimits
    {
        public static readonly int TeamJoinLimit = 10;
        public static readonly int TeamMaxSize = 20;
        public static readonly int VacanciesMaxCount = 100;
        public static readonly int VacancyTagMaxCount = 10;
    }

    public class WorkplaceService
    {
        private readonly IStorageService<string, CustomerInfo> _customerInfos;
        private readonly ItemLocker<string> _customerLocker = new();
        private readonly ItemLocker<int> _teamLocker = new();
        private readonly IStorageService<int, Team> _teams;
        private readonly IStorageService<int, Vacancy> _vacancies;
        private readonly ItemLocker<int> _vacancyLocker = new();

        public WorkplaceService(
            IStorageService<string, CustomerInfo> customerInfos,
            IStorageService<int, Team> teams,
            IStorageService<int, Vacancy> vacancies)
        {
            _customerInfos = customerInfos;
            _teams = teams;
            _vacancies = vacancies;
        }

        public static bool CanChangeRole(TeamPermissions callerPermission,
                             TeamPermissions subjectPermissions,
                             TeamPermissions newRole)
        {
            bool canDirect = callerPermission.HasFlag(TeamPermissions.Director)
                    && !subjectPermissions.HasFlag(TeamPermissions.Director);
            bool canManage = canDirect || callerPermission.HasFlag(TeamPermissions.Manager)
                    && !subjectPermissions.HasFlag(TeamPermissions.Manager);
            return newRole switch
            {
                TeamPermissions.Director
                    => canDirect,
                TeamPermissions.Manager
                    => canDirect,
                TeamPermissions.Editor => canManage,
                TeamPermissions.Commentator => canManage,
                TeamPermissions.Observer => canManage,
                _ => false
            };
        }

        public static bool CanKick(TeamPermissions callerPermission,
                             TeamPermissions subjectPermissions)
        {
            return (callerPermission.HasFlag(TeamPermissions.KickDirector)
                || (callerPermission.HasFlag(TeamPermissions.KickManager)
                && !subjectPermissions.HasFlag(TeamPermissions.Director))
                || (callerPermission.HasFlag(TeamPermissions.KickMember)
                && !subjectPermissions.HasFlag(TeamPermissions.Manager)));
        }

        public WorkplaceResult<int> CreateTeam(
                            string callerUsername, TeamProperties properties)
        {
            if (callerUsername is null)
            {
                return WorkplaceResult.FromServerError<int>();
            }
            using var _ = _customerLocker.Acquire(callerUsername);
            var infoResult = _customerInfos.Get(callerUsername);
            if (!infoResult.IsCompleted
                || infoResult.Value.AccountState != AccountStates.Active)
            {
                return WorkplaceResult.FromServerError<int>();
            }
            if (WorkplaceLimits.TeamJoinLimit <= infoResult.Value.JoinedTeamNames.Count)
            {
                return WorkplaceResult.FromError<int>(WorkplaceErrors.JoinLimitExceeded);
            }
            var team = new Team { Properties = properties };
            team.Permissions.Add(callerUsername, TeamPermissions.Director);
            var insertResult = _teams.Insert(team);
            if (!insertResult.IsCompleted)
            {
                return WorkplaceResult.FromServerError<int>();
            }
            var getResult = _teams.Get(insertResult.Value);
            if (!getResult.IsCompleted)
            {
                return WorkplaceResult.FromServerError<int>();
            }
            var teamId = getResult.Value.TeamId;
            infoResult.Value.JoinedTeamNames.Add(teamId, properties.Name);
            if (!_customerInfos.Update(callerUsername,
                                        infoResult.Value).IsCompleted)
            {
                return WorkplaceResult.FromServerError<int>();
            }
            return WorkplaceResult.FromValue(teamId);
        }

        public WorkplaceResult<int> CreateVacancy(string callerUsername,
            int teamId, VacancyProperties properties)
        {
            if (callerUsername is null)
            {
                return WorkplaceResult.FromServerError<int>();
            }
            using var _ = _teamLocker.Acquire(teamId);
            var teamResult = GetTeam(callerUsername, teamId);
            if (teamResult.HasError)
            {
                return WorkplaceResult.FromError<int>(teamResult.Error);
            }
            var team = teamResult.Value;
            if (!team.Permissions[callerUsername].HasFlag(
                    TeamPermissions.CreateVacancy))
            {
                return WorkplaceResult.FromError<int>(WorkplaceErrors.NoPermission);
            }
            if (WorkplaceLimits.VacanciesMaxCount <= team.VacancyCount)
            {
                return WorkplaceResult.FromError<int>(WorkplaceErrors.VacancyCountLimitExceeded);
            }
            team.VacancyCount++;
            if (!_teams.Update(teamId, team).IsCompleted)
            {
                return WorkplaceResult.FromServerError<int>();
            }
            var vacancy = new Vacancy
            {
                OwnerTeamId = teamId,
                Properties = properties
            };
            var insertResult = _vacancies.Insert(vacancy);
            if (!insertResult.IsCompleted)
            {
                team.VacancyCount--;
                _teams.Update(teamId, team);
                return WorkplaceResult.FromServerError<int>();
            }
            return WorkplaceResult.FromValue(insertResult.Value);
        }

        public WorkplaceResult DeleteVacancy(string callerUsername,
            int vacancyId)
        {
            var vacancyResult = GetVacancy(
                callerUsername, vacancyId, TeamPermissions.DeleteVacancy);
            if (vacancyResult.HasError)
            {
                return WorkplaceResult.FromError(vacancyResult.Error);
            }
            if (!_vacancies.Delete(vacancyResult.Value.VacancyId).IsCompleted)
            {
                return WorkplaceResult.FromServerError();
            }
            var teamId = vacancyResult.Value.OwnerTeamId;
            using var _ = _teamLocker.Acquire(teamId);
            var teamResult = _teams.Get(teamId);
            if (!teamResult.IsCompleted)
            {
                return WorkplaceResult.FromServerError();
            }
            teamResult.Value.VacancyCount--;
            if (!_teams.Update(teamId, teamResult.Value).IsCompleted)
            {
                return WorkplaceResult.FromServerError();
            }
            return WorkplaceResult.Succeed();
        }

        public WorkplaceResult DeleteVacancyNote(string callerUsername,
            int vacancyId, string noteOwnerUsername)
        {
            using var vacancyCertificate = _vacancyLocker.Acquire(vacancyId);
            var permissions = callerUsername == noteOwnerUsername
                ? TeamPermissions.CommentVacancy : TeamPermissions.ManageVacancyNotes;
            var vacancyResult = GetVacancy(
                callerUsername, vacancyId, permissions);
            if (vacancyResult.HasError)
            {
                return WorkplaceResult.FromError(vacancyResult.Error);
            }
            vacancyResult.Value.Notes.Remove(noteOwnerUsername);
            if (!_vacancies.Update(vacancyId, vacancyResult.Value).IsCompleted)
            {
                return WorkplaceResult.FromServerError();
            }
            return WorkplaceResult.Succeed();
        }

        public WorkplaceResult<Team> GetTeam(string callerUsername, int teamId)
        {
            if (callerUsername is null)
            {
                return WorkplaceResult.FromServerError<Team>();
            }
            var existsResult = _teams.HasKey(teamId);
            if (!existsResult.IsCompleted)
            {
                return WorkplaceResult.FromServerError<Team>();
            }
            if (!existsResult.Value)
            {
                return WorkplaceResult.FromError<Team>(WorkplaceErrors.ResourceNotFound);
            }
            var getResult = _teams.Get(teamId);
            if (!getResult.IsCompleted)
            {
                return WorkplaceResult.FromServerError<Team>();
            }
            if (!getResult.Value.HasMember(callerUsername))
            {
                return WorkplaceResult.FromError<Team>(WorkplaceErrors.ResourceNotFound);
            }
            return WorkplaceResult.FromValue(getResult.Value);
        }

        public WorkplaceResult<IEnumerable<Vacancy>> GetVacancies(
            string callerUsername, int teamId)
        {
            var teamResult = GetTeam(callerUsername, teamId);
            if (teamResult.HasError)
            {
                return WorkplaceResult.FromError<IEnumerable<Vacancy>>(teamResult.Error);
            }
            var selectionResult = _vacancies.Select(
                vacancy => vacancy.OwnerTeamId == teamId);
            if (!selectionResult.IsCompleted)
            {
                return WorkplaceResult.FromServerError<IEnumerable<Vacancy>>();
            }
            return WorkplaceResult.FromValue(selectionResult.Value);
        }

        public WorkplaceResult<Vacancy> GetVacancy(
            string callerUsername, int vacancyId,
            TeamPermissions requiredPermissions = TeamPermissions.None)
        {
            if (callerUsername is null)
            {
                return WorkplaceResult.FromServerError<Vacancy>();
            }
            var existsResult = _vacancies.HasKey(vacancyId);
            if (!existsResult.IsCompleted)
            {
                return WorkplaceResult.FromServerError<Vacancy>();
            }
            if (!existsResult.Value)
            {
                return WorkplaceResult.FromError<Vacancy>(WorkplaceErrors.ResourceNotFound);
            }
            var vacancyResult = _vacancies.Get(vacancyId);
            if (!vacancyResult.IsCompleted)
            {
                return WorkplaceResult.FromServerError<Vacancy>();
            }
            var teamResult = GetTeam(callerUsername, vacancyResult.Value.OwnerTeamId);
            if (teamResult.HasError)
            {
                return WorkplaceResult.FromError<Vacancy>(teamResult.Error);
            }
            if (!teamResult.Value.Permissions[callerUsername].HasFlag(
                    requiredPermissions))
            {
                return WorkplaceResult.FromError<Vacancy>(WorkplaceErrors.NoPermission);
            }
            return WorkplaceResult.FromValue(vacancyResult.Value);
        }

        public WorkplaceResult Invite(string callerUsername,
            int teamId, string subjectUsername)
        {
            if (callerUsername is null || subjectUsername is null)
            {
                return WorkplaceResult.FromServerError();
            }
            var existsResult = _customerInfos.HasKey(subjectUsername);
            if (!existsResult.IsCompleted)
            {
                return WorkplaceResult.FromServerError();
            }
            if (!existsResult.Value)
            {
                return WorkplaceResult.FromError(WorkplaceErrors.UserNotFound);
            }
            using var _ = _customerLocker.Acquire(callerUsername);
            using var teamCertificate = _teamLocker.Acquire(teamId);
            var infoResult = _customerInfos.Get(subjectUsername);
            if (!infoResult.IsCompleted
                || infoResult.Value.AccountState != AccountStates.Active)
            {
                return WorkplaceResult.FromServerError();
            }
            var teamResult = _teams.Get(teamId);
            if (!teamResult.IsCompleted)
            {
                return WorkplaceResult.FromServerError();
            }
            var team = teamResult.Value;
            if (team.HasMember(subjectUsername))
            {
                return WorkplaceResult.FromError(WorkplaceErrors.UserAlreadyJoined);
            }
            if (WorkplaceLimits.TeamJoinLimit <= infoResult.Value.JoinedTeamNames.Count)
            {
                return WorkplaceResult.FromError(WorkplaceErrors.JoinLimitExceeded);
            }
            if (WorkplaceLimits.TeamMaxSize <= team.Permissions.Count)
            {
                return WorkplaceResult.FromError(
                    WorkplaceErrors.TeamSizeLimitExceeded);
            }
            if (!team.HasMember(callerUsername))
            {
                return WorkplaceResult.FromServerError();
            }
            if (!team.Permissions[callerUsername].HasFlag(
                TeamPermissions.Invite))
            {
                return WorkplaceResult.FromError(WorkplaceErrors.NoPermission);
            }
            team.Permissions.Add(subjectUsername, TeamPermissions.None);
            if (!_teams.Update(teamId, team).IsCompleted)
            {
                return WorkplaceResult.FromServerError();
            }
            teamCertificate.ReportRelease();
            infoResult.Value.JoinedTeamNames.Add(team.TeamId, team.Properties.Name);
            if (!_customerInfos.Update(subjectUsername,
                                        infoResult.Value).IsCompleted)
            {
                return WorkplaceResult.FromServerError();
            }
            return WorkplaceResult.Succeed();
        }

        public WorkplaceResult Kick(string callerUsername,
                int teamId, string subjectUsername)
        {
            if (callerUsername is null || subjectUsername is null)
            {
                return WorkplaceResult.FromServerError();
            }
            using var teamCertificate = _teamLocker.Acquire(teamId);
            var teamResult = _teams.Get(teamId);
            if (!teamResult.IsCompleted)
            {
                return WorkplaceResult.FromServerError();
            }
            var team = teamResult.Value;
            var permissions = team.Permissions;
            if (!team.HasMember(callerUsername))
            {
                return WorkplaceResult.FromServerError();
            }
            if (!team.HasMember(subjectUsername))
            {
                return WorkplaceResult.FromError(WorkplaceErrors.UserNotFound);
            }
            if (!CanKick(permissions[callerUsername], permissions[subjectUsername]))
            {
                return WorkplaceResult.FromError(WorkplaceErrors.NoPermission);
            }
            team.Permissions.Remove(subjectUsername);
            if (!_teams.Update(teamId, team).IsCompleted)
            {
                return WorkplaceResult.FromServerError();
            }
            teamCertificate.ReportRelease();
            using var _ = _customerLocker.Acquire(subjectUsername);
            var infoResult = _customerInfos.Get(subjectUsername);
            if (infoResult.IsCompleted)
            {
                infoResult.Value.JoinedTeamNames.Remove(teamId);
                _customerInfos.Update(subjectUsername, infoResult.Value);
            }
            return WorkplaceResult.Succeed();
        }

        public WorkplaceResult Leave(string callerUsername,
            int teamId)
        {
            if (callerUsername is null)
            {
                return WorkplaceResult.FromServerError();
            }
            using var teamCertificate = _teamLocker.Acquire(teamId);
            var teamResult = _teams.Get(teamId);
            if (!teamResult.IsCompleted)
            {
                return WorkplaceResult.FromServerError();
            }
            var permissions = teamResult.Value.Permissions;
            if (permissions.Count == 1)
            {
                var selectionResult = _vacancies.Select(
                    vacancy => vacancy.OwnerTeamId == teamId);
                if (!selectionResult.IsCompleted)
                {
                    return WorkplaceResult.FromServerError();
                }
                var vacancies = selectionResult.Value.ToList();
                foreach (var vacancy in vacancies)
                {
                    if (!_vacancies.Delete(vacancy.VacancyId).IsCompleted)
                    {
                        return WorkplaceResult.FromServerError();
                    }
                }
                if (!_teams.Delete(teamId).IsCompleted)
                {
                    return WorkplaceResult.FromServerError();
                }
            }
            else
            {
                UpdatePermissonsWhenLeaving(permissions, callerUsername);
                if (!_teams.Update(teamId, teamResult.Value).IsCompleted)
                {
                    return WorkplaceResult.FromServerError();
                }
            }
            teamCertificate.ReportRelease();
            using var _ = _customerLocker.Acquire(callerUsername);
            var infoResult = _customerInfos.Get(callerUsername);
            if (infoResult.IsCompleted)
            {
                infoResult.Value.JoinedTeamNames.Remove(teamId);
                _customerInfos.Update(callerUsername, infoResult.Value);
            }
            return WorkplaceResult.Succeed();
        }

        public WorkplaceResult ModifyRole(string callerUsername,
            int teamId, string subjectUsername, TeamPermissions newRole)
        {
            if (callerUsername is null)
            {
                return WorkplaceResult.FromServerError();
            }
            using var _ = _teamLocker.Acquire(teamId);
            var teamResult = _teams.Get(teamId);
            if (!teamResult.IsCompleted)
            {
                return WorkplaceResult.FromServerError();
            }
            var team = teamResult.Value;
            var permissions = team.Permissions;
            if (!team.HasMember(callerUsername))
            {
                return WorkplaceResult.FromServerError();
            }
            if (!team.HasMember(subjectUsername))
            {
                return WorkplaceResult.FromError(WorkplaceErrors.UserNotFound);
            }
            if (!CanChangeRole(permissions[callerUsername],
                permissions[subjectUsername], newRole))
            {
                return WorkplaceResult.FromError(WorkplaceErrors.NoPermission);
            }
            team.Permissions[subjectUsername] = newRole;
            if (!_teams.Update(teamId, team).IsCompleted)
            {
                return WorkplaceResult.FromServerError();
            }
            return WorkplaceResult.Succeed();
        }

        public WorkplaceResult ModifyTeamProperties(
            string callerUsername, int teamId, TeamProperties properties)
        {
            if (callerUsername is null)
            {
                return WorkplaceResult.FromServerError();
            }
            using var teamCertificate = _teamLocker.Acquire(teamId);
            var teamResult = _teams.Get(teamId);
            if (!teamResult.IsCompleted)
            {
                return WorkplaceResult.FromServerError();
            }
            var team = teamResult.Value;
            var teamName = team.Properties.Name;
            if (!team.HasMember(callerUsername)
                || !team.Permissions[callerUsername].HasFlag(
                    TeamPermissions.ModifyTeamProperties))
            {
                return WorkplaceResult.FromError(WorkplaceErrors.NoPermission);
            }
            team.Properties = properties;
            if (!_teams.Update(teamId, team).IsCompleted)
            {
                return WorkplaceResult.FromServerError();
            }
            teamCertificate.ReportRelease();
            if (properties.Name != teamName)
            {
                foreach (var username in team.Permissions.Keys)
                {
                    using var _ = _customerLocker.Acquire(username);
                    var infoResult = _customerInfos.Get(username);
                    if (infoResult.IsCompleted &&
                        infoResult.Value.JoinedTeamNames.ContainsKey(teamId))
                    {
                        infoResult.Value.JoinedTeamNames[teamId] = properties.Name;
                        _customerInfos.Update(username, infoResult.Value);
                    }
                }
            }
            return WorkplaceResult.Succeed();
        }

        public WorkplaceResult ModifyVacancyNote(string callerUsername,
            int vacancyId, VacancyNote note)
        {
            using var vacancyCertificate = _vacancyLocker.Acquire(vacancyId);
            var vacancyResult = GetVacancy(
                callerUsername, vacancyId, TeamPermissions.CommentVacancy);
            if (vacancyResult.HasError)
            {
                return WorkplaceResult.FromError(vacancyResult.Error);
            }
            vacancyResult.Value.Notes[callerUsername] = note;
            vacancyResult.Value.ReportNoteUpdated();
            if (!_vacancies.Update(vacancyId, vacancyResult.Value).IsCompleted)
            {
                return WorkplaceResult.FromServerError();
            }
            return WorkplaceResult.Succeed();
        }

        public WorkplaceResult ModifyVacancyProperties(string callerUsername,
                    int vacancyId, VacancyProperties properties)
        {
            using var vacancyCertificate = _vacancyLocker.Acquire(vacancyId);
            var vacancyResult = GetVacancy(
                callerUsername, vacancyId, TeamPermissions.ModifyVacancy);
            if (vacancyResult.HasError)
            {
                return WorkplaceResult.FromError(vacancyResult.Error);
            }
            var vacancy = vacancyResult.Value;
            vacancy.Properties = properties;
            if (!_vacancies.Update(vacancyId, vacancy).IsCompleted)
            {
                return WorkplaceResult.FromServerError();
            }
            return WorkplaceResult.Succeed();
        }

        private static void UpdatePermissonsWhenLeaving(
            Dictionary<string, TeamPermissions> permissions,
            string leavingUsername)
        {
            if (!permissions.ContainsKey(leavingUsername))
            {
                return;
            }
            permissions.Remove(leavingUsername);
            if (!permissions.Values.Any(
                p => p.HasFlag(TeamPermissions.Director)))
            {
                bool promoteAll = !permissions.Values.Any(
                    p => p.HasFlag(TeamPermissions.Manager));
                foreach (var key in permissions.Keys)
                {
                    if (promoteAll || permissions[key].HasFlag(
                        TeamPermissions.Manager))
                    {
                        permissions[key] |= TeamPermissions.Director;
                    }
                }
            }
        }
    }
}
