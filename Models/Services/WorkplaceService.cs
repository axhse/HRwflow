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
        private readonly ItemLocker<string> _customerLocker = new();
        private readonly ItemLocker<int> _teamLocker = new();
        private readonly IStorageService<int, Team> _teams;

        public WorkplaceService(
            IStorageService<string, CustomerInfo> customerInfos,
            IStorageService<int, Team> teams)
        {
            _customerInfos = customerInfos;
            _teams = teams;
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
            team.Permissions.Add(callerUsername, TeamPermissions.Direct);
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
            throw new NotImplementedException();
        }

        public WorkplaceResult DeleteVacancy(string callerUsername,
            int teamId, int vacancyId)
        {
            throw new NotImplementedException();
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
            if (!getResult.Value.Permissions.ContainsKey(callerUsername))
            {
                return WorkplaceResult.FromError<Team>(WorkplaceErrors.ResourceNotFound);
            }
            return WorkplaceResult.FromValue(getResult.Value);
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
            if (team.Permissions.ContainsKey(subjectUsername))
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
            if (!team.Permissions.ContainsKey(callerUsername))
            {
                return WorkplaceResult.FromServerError();
            }
            if (!team.Permissions[callerUsername].HasFlag(TeamPermissions.Manage))
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
            if (!permissions.ContainsKey(callerUsername))
            {
                return WorkplaceResult.FromServerError();
            }
            if (!permissions.ContainsKey(subjectUsername))
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

        public WorkplaceResult ModifyPermissions(string callerUsername,
            int teamId, string subjectUsername, TeamPermissions permissions)
        {
            throw new NotImplementedException();
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
            if (!team.Permissions.ContainsKey(callerUsername)
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
            var memberUsernames = new List<string>(team.Permissions.Keys);
            teamCertificate.ReportRelease();
            if (properties.Name != teamName)
            {
                foreach (var username in memberUsernames)
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

        public WorkplaceResult ModifyVacancyProperties(string callerUsername,
            int teamId, int vacancyId, VacancyProperties vacancyProperties)
        {
            throw new NotImplementedException();
        }

        private static bool CanKick(TeamPermissions callerPermission,
                             TeamPermissions subjectPermissions)
        {
            return (callerPermission.HasFlag(TeamPermissions.Direct)
                && !subjectPermissions.HasFlag(TeamPermissions.Direct))
                || (callerPermission.HasFlag(TeamPermissions.Manage)
                && !subjectPermissions.HasFlag(TeamPermissions.Manage));
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
            if (!permissions.Values.Any(p => p.HasFlag(TeamPermissions.Direct)))
            {
                bool promoteAll =
                    !permissions.Values.Any(p => p.HasFlag(TeamPermissions.Manage));
                foreach (var key in permissions.Keys)
                {
                    if (promoteAll || permissions[key].HasFlag(TeamPermissions.Manage))
                    {
                        permissions[key] |= TeamPermissions.Direct;
                    }
                }
            }
        }
    }
}
