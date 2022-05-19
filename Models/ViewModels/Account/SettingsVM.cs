namespace HRwflow.Models
{
    public class SettingsVM : CustomerVM
    {
        public bool HasErrors => !IsNameCorrect;
        public bool IsNameCorrect { get; set; } = true;
        public bool IsPasswordConfirmationCorrect { get; set; } = true;
        public bool IsPasswordCorrect { get; set; } = true;
    }
}
