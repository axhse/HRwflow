namespace HRwflow.Models
{
    public class SignUpVM
    {
        public string DefaultUsername { get; set; } = string.Empty;

        public bool HasErrors => !IsUsernameCorrect || !IsPasswordCorrect
                || !IsPasswordConfirmationCorrect || !IsUsernameUnused;

        public bool IsPasswordConfirmationCorrect { get; set; } = true;
        public bool IsPasswordCorrect { get; set; } = true;
        public bool IsUsernameCorrect { get; set; } = true;
        public bool IsUsernameUnused { get; set; } = true;
    }
}
