using System.Linq;

namespace HRwflow.Models
{
    public class TeamInfo
    {
        public TeamInfo()
        { }

        public TeamInfo(string username, Team team)
        {
            if (!team.Permissions.TryGetValue(username, out TeamPermissions permissions))
            {
                permissions = TeamPermissions.None;
            }
            ActiveVacancyCount = team.Vacancies.Values.Count(
                v => v.Properties.State == VacancyState.Active);
            Permissions = permissions;
            Properties = team.Properties;
            TeamId = team.TeamId;
        }

        public int ActiveVacancyCount { get; set; }
        public TeamPermissions Permissions { get; set; }
        public TeamProperties Properties { get; set; }
        public int TeamId { get; set; }
    }
}
