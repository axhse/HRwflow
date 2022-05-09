using System;

namespace HRwflow.Models
{
    public static class WorkplaceLimits
    {
        public static readonly int TeamJoinLimit = 10;
        public static readonly int TeamMaxSize = 20;
        public static readonly int VacanciesMaxCount = 100;
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
            var customerCertificate = _customerLocker.Acquire(callerUsername);
            AcquireCertificate<int> teamCertificate = null;
            try
            {
                var infoResult = _customerInfos.Get(callerUsername);
                if (!infoResult.IsCompleted)
                {
                    return WorkplaceResult.FromServerError<int>();
                }
                if (WorkplaceLimits.TeamJoinLimit <= infoResult.Value.JoinedTeamNames.Count)
                {
                    return WorkplaceResult.FromError<int>(WorkplaceErrors.JoinLimitExceeded);
                }
                var team = new Team { Properties = properties };
                team.Permissions.Add(callerUsername, TeamPermissions.All);
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
                teamCertificate = _teamLocker.Acquire(teamId);
                infoResult.Value.JoinedTeamNames.Add(teamId, properties.Name);
                if (!_customerInfos.Update(callerUsername,
                                           infoResult.Value).IsCompleted)
                {
                    return WorkplaceResult.FromServerError<int>();
                }
                return WorkplaceResult.FromValue(teamId);
            }
            finally
            {
                customerCertificate.ReportRelease();
                teamCertificate?.ReportRelease();
            }
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

        public WorkplaceResult<TeamInfo> GetTeamInfo(string callerUsername, int teamId)
        {
            var teamCertificate = _teamLocker.Acquire(teamId);
            try
            {
                var existsResult = _teams.HasKey(teamId);
                if (!existsResult.IsCompleted)
                {
                    return WorkplaceResult.FromServerError<TeamInfo>();
                }
                if (!existsResult.Value)
                {
                    return WorkplaceResult.FromError<TeamInfo>(
                        WorkplaceErrors.ResourceNotFound);
                }
                var getResult = _teams.Get(teamId);
                if (!getResult.IsCompleted)
                {
                    return WorkplaceResult.FromServerError<TeamInfo>();
                }
                if (!getResult.Value.Permissions.ContainsKey(callerUsername))
                {
                    return WorkplaceResult.FromError<TeamInfo>(WorkplaceErrors.ResourceNotFound);
                }
                return WorkplaceResult.FromValue(new TeamInfo(callerUsername, getResult.Value));
            }
            finally
            {
                teamCertificate.ReportRelease();
            }
        }

        public WorkplaceResult Invite(string callerUsername,
            int teamId, string targetUsername)
        {
            throw new NotImplementedException();
        }

        public WorkplaceResult Kick(string callerUsername,
            int teamId, string targetUsername)
        {
            throw new NotImplementedException();
        }

        public WorkplaceResult ModifyPermissions(string callerUsername,
            int teamId, string targetUsername, TeamPermissions permissions)
        {
            throw new NotImplementedException();
        }

        public WorkplaceResult ModifyTeamProperties(
            string callerUsername, int teamId, TeamProperties properties)
        {
            var teamCertificate = _teamLocker.Acquire(teamId);
            try
            {
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
                if (properties.Name != teamName)
                {
                    // FIXME: Create background worker
                    foreach (var username in team.Permissions.Keys)
                    {
                        var customerCertificate = _customerLocker.Acquire(username);
                        try
                        {
                            var infoResult = _customerInfos.Get(username);
                            if (infoResult.IsCompleted)
                            {
                                var info = infoResult.Value;
                                info.JoinedTeamNames[teamId] = properties.Name;
                                _customerInfos.Update(username, info);
                            }
                        }
                        finally
                        {
                            customerCertificate.ReportRelease();
                        }
                    }
                }
                return WorkplaceResult.Succeed();
            }
            finally
            {
                teamCertificate.ReportRelease();
            }
        }

        public WorkplaceResult ModifyVacancyProperties(string callerUsername,
            int teamId, int vacancyId, VacancyProperties vacancyProperties)
        {
            throw new NotImplementedException();
        }
    }
}
