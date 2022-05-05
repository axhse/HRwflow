namespace HRwflow.Models
{
    public class SignInVM
    {
        private string _defaultUsername;

        public string DefaultUsername
        {
            get => _defaultUsername is null ? string.Empty : _defaultUsername;
            set => _defaultUsername = value;
        }

        public bool HasErrors => !IsUserExists || !IsPasswordValid;
        public bool IsPasswordValid { get; set; } = true;
        public bool IsUserExists { get; set; } = true;
        public bool RememberMeChecked { get; set; } = true;
    }
}
