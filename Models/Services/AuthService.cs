using System.Threading.Tasks;
using HRwflow.Models.Services;

namespace HRwflow.Models
{
    public class AuthService : IAuthService
    {
        private readonly IStorageService<string, AuthData> _storageService;

        public AuthService(IStorageService<string, AuthData> storageService)
        {
            _storageService = storageService;
        }

        public async Task<TaskResult> Delete(string username)
        {
            var result = await _storageService.Delete(username);
            if (!result.IsCompleted)
            {
                return await Task.FromResult(TaskResult.Uncompleted());
            }
            return await Task.FromResult(TaskResult.FromCondition(result.IsSuccessful));
        }

        public async Task<TaskResult> SignIn(string username, string password)
        {
            var result = await _storageService.Get(username);
            if (!result.IsCompleted)
            {
                return await Task.FromResult(TaskResult.Uncompleted());
            }
            bool isSuccessful = result.IsSuccessful
                && AuthData.GetPasswordHash(password) == result.Value.PasswordHash;
            return await Task.FromResult(TaskResult.FromCondition(isSuccessful));
        }

        public async Task<TaskResult> SignUp(string username, string password)
        {
            if (username is null || password is null)
            {
                return await Task.FromResult(TaskResult.Uncompleted());
            }
            var authData = new AuthData
            {
                Username = username,
                PasswordHash = AuthData.GetPasswordHash(password)
            };
            var result = await _storageService.Insert(authData);
            if (!result.IsCompleted)
            {
                return await Task.FromResult(TaskResult.Uncompleted());
            }
            return await Task.FromResult(TaskResult.FromCondition(result.IsSuccessful));
        }
    }
}
