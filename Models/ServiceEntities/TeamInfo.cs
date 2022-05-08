namespace HRwflow.Models
{
    public class TeamInfo
    {
        public TeamInfo(string username, Team team)
        {
            if (!team.Permissions.TryGetValue(username, out TeamPermissions permissions))
            {
                permissions = TeamPermissions.None;
            }
            Permissions = permissions;
            Properties = team.Properties;
            TeamId = team.TeamId;
        }

        public TeamPermissions Permissions { get; set; }
        public TeamProperties Properties { get; set; }
        public int TeamId { get; set; }
    }
}
