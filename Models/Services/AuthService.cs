namespace HRwflow.Models
{
    public class AuthService : IAuthService
    {
        private readonly IStorageService<string, AuthInfo> _authInfos;

        public AuthService(IStorageService<string, AuthInfo> authInfos)
        {
            _authInfos = authInfos;
        }

        public TaskResult DeleteAccount(string username)
        {
            return _authInfos.Delete(username);
        }

        public TaskResult<bool> IsUserExists(string username)
        {
            if (username is null)
            {
                return TaskResult<bool>.Uncompleted();
            }
            return _authInfos.HasKey(username);
        }

        public TaskResult<bool> SignIn(string username, string password)
        {
            if (!Customer.IsUsernameCorrect(username)
                || !AuthInfo.IsPasswordCorrect(password))
            {
                return TaskResult.FromValue(false);
            }
            var result = _authInfos.Get(username);
            if (!result.IsCompleted)
            {
                return TaskResult<bool>.Uncompleted();
            }
            return TaskResult.FromValue(
                AuthInfo.CalculateHash(password) == result.Value.PasswordHash);
        }

        public TaskResult SignUp(string username, string password)
        {
            if (!Customer.IsUsernameCorrect(username)
                || !AuthInfo.IsPasswordCorrect(password))
            {
                return TaskResult.Uncompleted();
            }
            var info = new AuthInfo
            {
                Username = username,
                PasswordHash = AuthInfo.CalculateHash(password)
            };
            return _authInfos.Insert(username, info);
        }

        public TaskResult UpdatePassword(string username, string password)
        {
            if (!Customer.IsUsernameCorrect(username)
                || !AuthInfo.IsPasswordCorrect(password))
            {
                return TaskResult.Uncompleted();
            }
            var result = _authInfos.Get(username);
            if (!result.IsCompleted)
            {
                return TaskResult.Uncompleted();
            }
            result.Value.PasswordHash = AuthInfo.CalculateHash(password);
            return _authInfos.Update(username, result.Value);
        }
    }
}
