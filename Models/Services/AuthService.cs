using System;

namespace HRwflow.Models
{
    public class AuthService
    {
        private readonly IStorageService<string, AuthInfo> _authInfos;

        public AuthService(IStorageService<string, AuthInfo> authInfos)
        {
            _authInfos = authInfos;
        }

        public void DeleteAccount(string username)
        {
            CheckUsername(username);
            _authInfos.Delete(username);
        }

        public bool IsAccountExists(string username)
        {
            return Customer.IsUsernameCorrect(username)
                && _authInfos.HasKey(username);
        }

        public ActionResult<SignInErrors> SignIn(
            string username, string password)
        {
            if (!Customer.IsUsernameCorrect(username)
                || !_authInfos.TryFind(username, out var info))
            {
                return new(SignInErrors.AccountNotFound);
            }
            if (AuthInfo.IsPasswordCorrect(password)
                && AuthInfo.CalculateHash(password)
                    == info.PasswordHash)
            {
                return new();
            }
            return new(SignInErrors.PasswordIsWrong);
        }

        public ActionResult<SignUpErrors> SignUp(
            string username, string password)
        {
            CheckUsername(username);
            CheckPassword(password);
            var info = new AuthInfo
            {
                Username = username,
                PasswordHash
                    = AuthInfo.CalculateHash(password)
            };
            if (!_authInfos.TryInsert(username, info))
            {
                return new(SignUpErrors.UsernameIsTaken);
            }
            return new();
        }

        public void UpdatePassword(
            string username, string password)
        {
            CheckUsername(username);
            CheckPassword(password);
            if (!_authInfos.TryFind(username, out var info))
            {
                throw new ArgumentException($"There is" +
                    $" no account with username {username}.");
            }
            info.PasswordHash
                = AuthInfo.CalculateHash(password);
            _authInfos.Update(username, info);
        }

        private static void CheckUsername(string username)
        {
            if (!Customer.IsUsernameCorrect(username))
            {
                throw new ArgumentException(
                    $"{nameof(username)} is not correct.");
            }
        }

        private static void CheckPassword(string password)
        {
            if (!AuthInfo.IsPasswordCorrect(password))
            {
                throw new ArgumentException(
                    $"{nameof(password)} is not correct.");
            }
        }
    }
}
