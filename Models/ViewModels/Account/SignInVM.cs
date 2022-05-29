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

        public SignInErrors Error
        {
            set
            {
                IsPasswordRight &=
                    value != SignInErrors.PasswordIsWrong;
                IsAccountExists &=
                    value != SignInErrors.AccountNotFound;
            }
        }

        public bool HasErrors
            => !IsAccountExists || !IsPasswordRight;

        public bool IsPasswordRight { get; set; } = true;
        public bool IsAccountExists { get; set; } = true;
        public bool IsRememberMeChecked { get; set; } = true;
    }
}
