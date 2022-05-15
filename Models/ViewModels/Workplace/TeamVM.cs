namespace HRwflow.Models
{
    public class TeamVM
    {
        public TeamVM(Team team = null, string username = null)
        {
            Team = team;
            Username = username;
        }

        public Team Team { get; set; }
        public string Username { get; set; }
    }
}
