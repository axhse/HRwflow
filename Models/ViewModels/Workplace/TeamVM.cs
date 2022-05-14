namespace HRwflow.Models
{
    public class TeamVM
    {
        public TeamVM(Team team = null)
        {
            Team = team;
        }

        public Team Team { get; set; }
    }
}
