namespace HRwflow.Models
{
    public class EditTeamPropertiesVM
    {
        public bool HasErrors => !NameIsCorrect;
        public bool NameIsCorrect { get; set; } = true;
        public TeamProperties TeamProperties { get; set; }
    }
}
