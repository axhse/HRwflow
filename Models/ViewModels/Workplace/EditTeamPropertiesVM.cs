namespace HRwflow.Models
{
    public class EditTeamPropertiesVM
    {
        public EditTeamPropertiesVM(TeamProperties properties = default)
        {
            TeamProperties = properties;
        }

        public bool HasErrors => !NameIsCorrect;
        public bool NameIsCorrect { get; set; } = true;
        public int TeamId { get; set; }
        public TeamProperties TeamProperties { get; set; }
    }
}
