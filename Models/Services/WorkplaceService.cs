using System;
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
        private readonly ItemLocker<string> _infoLocker = new();
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

        public static bool CanChangeRole(
            TeamPermissions callerPermission,
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

        public static bool CanKick(
            TeamPermissions callerPermission,
            TeamPermissions subjectPermissions)
        {
            return (callerPermission.HasFlag(TeamPermissions.KickDirector)
                || (callerPermission.HasFlag(TeamPermissions.KickManager)
                && !subjectPermissions.HasFlag(TeamPermissions.Director))
                || (callerPermission.HasFlag(TeamPermissions.KickMember)
                && !subjectPermissions.HasFlag(TeamPermissions.Manager)));
        }

        public FuncResult<int, TeamCreationErrors> CreateTeam(
            string callerUsername,
            TeamProperties properties = new())
        {
            using var _ = _infoLocker.Acquire(callerUsername);
            var info = _customerInfos.Find(callerUsername);
            if (info.AccountState == AccountStates.OnDeletion)
            {
                throw new InvalidOperationException(
                    "Customer's account is in the process" +
                    " of deletion.");
            }
            if (WorkplaceLimits.TeamJoinLimit
                <= info.JoinedTeamNames.Count)
            {
                return new(TeamCreationErrors.JoinLimitExceeded);
            }
            var team = new Team { Properties = properties };
            team.Permissions.Add(callerUsername, TeamPermissions.Director);
            var teamId = _teams.Insert(team);
            info.JoinedTeamNames.Add(teamId, properties.Name);
            _customerInfos.Update(callerUsername, info);
            return new(teamId);
        }

        public FuncResult<int, VacancyCreationErrors> CreateVacancy(
            string callerUsername, int teamId,
            VacancyProperties properties = new())
        {
            using var _ = _teamLocker.Acquire(teamId);
            if (!_teams.TryFind(teamId, out var team)
                || !team.MemberHasPermissions(callerUsername,
                TeamPermissions.CommentVacancy))
            {
                throw AccessException();
            }
            if (WorkplaceLimits.VacanciesMaxCount <= team.VacancyCount)
            {
                return new(VacancyCreationErrors.VacancyCountLimitExceeded);
            }
            var vacancy = new Vacancy
            {
                OwnerTeamId = teamId,
                Properties = properties
            };
            var vacancyId = _vacancies.Insert(vacancy);
            team.VacancyCount++;
            _teams.Update(teamId, team);
            return new(vacancyId);
        }

        public void DeleteVacancy(
            string callerUsername, int vacancyId)
        {
            using var _ = _vacancyLocker.Acquire(vacancyId);
            if (!GetVacancy(callerUsername, vacancyId,
                TeamPermissions.DeleteVacancy)
                .TryGetValue(out var vacancy))
            {
                throw AccessException();
            }
            _vacancies.Delete(vacancy.VacancyId);
            if (_teams.TryFind(vacancy.OwnerTeamId, out var team))
            {
                team.VacancyCount--;
                _teams.Update(team.TeamId, team);
            }
        }

        public void DeleteVacancyNote(string callerUsername,
            int vacancyId, string noteOwnerUsername)
        {
            using var _ = _vacancyLocker.Acquire(vacancyId);
            var permissions =
                callerUsername == noteOwnerUsername
                ? TeamPermissions.CommentVacancy
                : TeamPermissions.ManageVacancyNotes;
            if (!GetVacancy(callerUsername, vacancyId,
                permissions).TryGetValue(out var vacancy))
            {
                throw AccessException();
            }
            vacancy.Notes.Remove(noteOwnerUsername);
            _vacancies.Update(vacancyId, vacancy);
        }

        public FuncResult<Team, CommonErrors> GetTeam(
            string callerUsername, int teamId,
            TeamPermissions requiredPermissions)
        {
            if (!_teams.TryFind(teamId, out var team))
            {
                return new(CommonErrors.ResourceNotFound);
            }
            if (!team.MemberHasPermissions(
                callerUsername, requiredPermissions))
            {
                return new(CommonErrors.NoPermission);
            }
            return new(team);
        }

        public FuncResult<IEnumerable<Vacancy>, CommonErrors> GetVacancies(
            string callerUsername, int teamId,
            TeamPermissions requiredPermissions)
        {
            var result = GetTeam(callerUsername,
                teamId, requiredPermissions);
            if (result.HasError)
            {
                return new(result.Error);
            }
            return new(_vacancies.Select(
                vacancy => vacancy.OwnerTeamId == teamId));
        }

        public FuncResult<Vacancy, CommonErrors> GetVacancy(
            string callerUsername, int vacancyId,
            TeamPermissions requiredPermissions)
        {
            if (!_vacancies.TryFind(vacancyId, out var vacancy))
            {
                return new(CommonErrors.ResourceNotFound);
            }
            var result = GetTeam(callerUsername,
                vacancy.OwnerTeamId, requiredPermissions);
            if (result.HasError)
            {
                return new(result.Error);
            }
            return new(vacancy);
        }

        public ActionResult<InvitationErrors> Invite(
            string callerUsername, int teamId,
            string subjectUsername)
        {
            using var teamCertificate
                = _teamLocker.Acquire(teamId);
            if (!GetTeam(callerUsername, teamId,
                TeamPermissions.Invite).TryGetValue(
                out var team))
            {
                throw AccessException();
            }
            using var _ = _infoLocker.Acquire(callerUsername);
            if (!_customerInfos.TryFind(subjectUsername,
                out var info) || info.AccountState
                == AccountStates.OnDeletion)
            {
                return new(InvitationErrors.UserNotFound);
            }
            if (team.HasMember(subjectUsername))
            {
                return new(InvitationErrors.UserAlreadyJoined);
            }
            if (WorkplaceLimits.TeamMaxSize
                <= team.Permissions.Count)
            {
                return new(InvitationErrors.TeamSizeLimitExceeded);
            }
            if (WorkplaceLimits.TeamJoinLimit
                <= info.JoinedTeamNames.Count)
            {
                return new(InvitationErrors.JoinLimitExceeded);
            }
            team.Permissions.Add(subjectUsername, TeamPermissions.None);
            _teams.Update(teamId, team);
            teamCertificate.ReportRelease();
            info.JoinedTeamNames.Add(teamId, team.Properties.Name);
            _customerInfos.Update(subjectUsername, info);
            return new();
        }

        public void Kick(string callerUsername,
                int teamId, string subjectUsername)
        {
            using var teamCertificate
                = _teamLocker.Acquire(teamId);
            if (!GetTeam(callerUsername, teamId,
                TeamPermissions.None).TryGetValue(
                out var team)
                || !team.HasMember(subjectUsername)
                || !CanKick(team.Permissions[callerUsername],
                    team.Permissions[subjectUsername]))
            {
                throw AccessException();
            }
            var permissions = team.Permissions;
            team.Permissions.Remove(subjectUsername);
            _teams.Update(teamId, team);
            teamCertificate.ReportRelease();
            using var _ = _infoLocker.Acquire(subjectUsername);
            if (_customerInfos.TryFind(subjectUsername,
                out var info) && info.AccountState
                != AccountStates.OnDeletion)
            {
                info.JoinedTeamNames.Remove(teamId);
                _customerInfos.Update(subjectUsername, info);
            }
        }

        public void Leave(string callerUsername, int teamId)
        {
            using var teamCertificate
                = _teamLocker.Acquire(teamId);
            if (!GetTeam(callerUsername, teamId,
                TeamPermissions.None).TryGetValue(
                out var team))
            {
                throw AccessException();
            }
            var permissions = team.Permissions;
            if (permissions.Count == 1)
            {
                var vacancies = _vacancies.Select(
                    vacancy => vacancy.OwnerTeamId == teamId)
                    .ToList();
                foreach (var vacancy in vacancies)
                {
                    _vacancies.Delete(vacancy.VacancyId);
                }
                _teams.Delete(teamId);
            }
            else
            {
                UpdatePermissonsWhenLeaving(permissions, callerUsername);
                _teams.Update(teamId, team);
            }
            teamCertificate.ReportRelease();
            using var _ = _infoLocker.Acquire(callerUsername);
            if (_customerInfos.TryFind(
                callerUsername, out var info))
            {
                info.JoinedTeamNames.Remove(teamId);
                _customerInfos.Update(callerUsername, info);
            }
        }

        public void ModifyRole(string callerUsername, int teamId,
            string subjectUsername, TeamPermissions newRole)
        {
            using var _ = _teamLocker.Acquire(teamId);
            if (!GetTeam(callerUsername, teamId,
                TeamPermissions.None).TryGetValue(out var team)
                || !team.HasMember(subjectUsername)
                || !CanChangeRole(
                    team.Permissions[callerUsername],
                    team.Permissions[subjectUsername],
                    newRole))
            {
                throw AccessException();
            }
            team.Permissions[subjectUsername] = newRole;
            _teams.Update(teamId, team);
        }

        public void ModifyTeamProperties(string callerUsername,
            int teamId, TeamProperties properties)
        {
            using var teamCertificate
                = _teamLocker.Acquire(teamId);
            if (!GetTeam(callerUsername, teamId,
                TeamPermissions.ModifyTeamProperties)
                .TryGetValue(out var team))
            {
                throw AccessException();
            }
            var teamName = team.Properties.Name;
            if (properties.Equals(team.Properties))
            {
                return;
            }
            team.Properties = properties;
            _teams.Update(teamId, team);
            teamCertificate.ReportRelease();
            if (properties.Name != teamName)
            {
                foreach (var username in team.Permissions.Keys)
                {
                    using var _ = _infoLocker.Acquire(username);
                    if (_customerInfos.TryFind(
                        username, out var info))
                    {
                        if (info.JoinedTeamNames.ContainsKey(
                            teamId))
                        {
                            info.JoinedTeamNames[teamId]
                                = properties.Name;
                            _customerInfos.Update(
                                username, info);
                        }
                    }
                }
            }
        }

        public void ModifyVacancyNote(string callerUsername,
            int vacancyId, VacancyNote note)
        {
            using var _ = _vacancyLocker.Acquire(vacancyId);
            if (!GetVacancy(callerUsername,
                vacancyId, TeamPermissions.CommentVacancy)
                .TryGetValue(out var vacancy))
            {
                throw AccessException();
            }
            vacancy.Notes[callerUsername] = note;
            vacancy.ReportNoteUpdated();
            _vacancies.Update(vacancyId, vacancy);
        }

        public void ModifyVacancyProperties(string callerUsername,
                    int vacancyId, VacancyProperties properties)
        {
            using var _ = _vacancyLocker.Acquire(vacancyId);
            if (!GetVacancy(callerUsername, vacancyId,
                TeamPermissions.ModifyVacancy).TryGetValue(
                out var vacancy))
            {
                throw AccessException();
            }
            if (properties.Equals(vacancy.Properties))
            {
                return;
            }
            vacancy.Properties = properties;
            _vacancies.Update(vacancyId, vacancy);
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

        private static InvalidOperationException AccessException()
            => new("The member has no permission for this" +
                " action or the target object is unavaliable.");
    }
}
