namespace HRwflow.Models
{
    public class SignUpVM
    {
        private string _defaultUsername;

        public string DefaultUsername
        {
            get => _defaultUsername is null ? string.Empty : _defaultUsername;
            set => _defaultUsername = value;
        }

        public bool HasErrors => !IsUsernameCorrect || !IsPasswordCorrect
                || !IsPasswordConfirmationCorrect || !IsUsernameUnused;

        public bool IsPasswordConfirmationCorrect { get; set; } = true;
        public bool IsPasswordCorrect { get; set; } = true;
        public bool IsUsernameCorrect { get; set; } = true;
        public bool IsUsernameUnused { get; set; } = true;
    }
}
