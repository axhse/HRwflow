namespace HRwflow.Models
{
    public class SettingsVM : CustomerVM
    {
        public bool HasErrors => !NameIsCorrect;
        public bool NameIsCorrect { get; set; } = true;
    }
}
