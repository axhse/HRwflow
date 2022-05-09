namespace HRwflow.Models
{
    public class AuthService : IAuthService
    {
        private readonly IStorageService<string, AuthCertificate> _certificates;

        public AuthService(IStorageService<string, AuthCertificate> storageService)
        {
            _certificates = storageService;
        }

        public TaskResult DeleteAccount(string username)
        {
            return _certificates.Delete(username);
        }

        public TaskResult<bool> IsUserExists(string username)
        {
            return _certificates.HasKey(username);
        }

        public TaskResult<bool> SignIn(string username, string password)
        {
            if (!Customer.PasswordIsCorrect(password))
            {
                return TaskResult<bool>.Uncompleted();
            }
            var result = _certificates.Get(username);
            if (!result.IsCompleted)
            {
                return TaskResult<bool>.Uncompleted();
            }
            return TaskResult<bool>.FromValue(
                AuthCertificate.CalculateHash(password) == result.Value.PasswordHash);
        }

        public TaskResult SignUp(string username, string password)
        {
            if (!Customer.UsernameIsCorrect(username)
                || !Customer.PasswordIsCorrect(password))
            {
                return TaskResult.Uncompleted();
            }
            var certificate = new AuthCertificate
            {
                Username = username,
                PasswordHash = AuthCertificate.CalculateHash(password)
            };
            return _certificates.Insert(username, certificate);
        }

        public TaskResult UpdatePassword(string username, string password)
        {
            if (!Customer.PasswordIsCorrect(password))
            {
                return TaskResult.Uncompleted();
            }
            var result = _certificates.Get(username);
            if (!result.IsCompleted)
            {
                return TaskResult.Uncompleted();
            }
            var certificate = new AuthCertificate
            {
                Username = username,
                PasswordHash = AuthCertificate.CalculateHash(password)
            };
            return _certificates.Update(username, certificate);
        }
    }
}
