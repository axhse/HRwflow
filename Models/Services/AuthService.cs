using System.Threading.Tasks;

namespace HRwflow.Models
{
    public class AuthService : IAuthService
    {
        private readonly IStorageService<string, AuthCertificate> _certificates;

        public AuthService(IStorageService<string, AuthCertificate> storageService)
        {
            _certificates = storageService;
        }

        public async Task<TaskResult> Delete(string username)
        {
            if (!Customer.UsernameIsCorrect(username))
            {
                return await Task.FromResult(TaskResult.Unsuccessful());
            }
            var result = await _certificates.Delete(username);
            if (!result.IsCompleted)
            {
                return await Task.FromResult(TaskResult.Uncompleted());
            }
            return await Task.FromResult(TaskResult.FromCondition(result.IsSuccessful));
        }

        public async Task<TaskResult> SignIn(string username, string password)
        {
            if (!Customer.UsernameIsCorrect(username) || !Customer.PasswordIsCorrect(password))
            {
                return await Task.FromResult(TaskResult.Unsuccessful());
            }
            var result = await _certificates.Get(username);
            if (!result.IsCompleted)
            {
                return await Task.FromResult(TaskResult.Uncompleted());
            }
            bool isSuccessful = result.IsSuccessful
                && AuthCertificate.CalculateHash(password) == result.Value.PasswordHash;
            return await Task.FromResult(TaskResult.FromCondition(isSuccessful));
        }

        public async Task<TaskResult> SignUp(string username, string password)
        {
            if (!Customer.UsernameIsCorrect(username) || !Customer.PasswordIsCorrect(password))
            {
                return await Task.FromResult(TaskResult.Unsuccessful());
            }
            var result = await _certificates.Insert(new AuthCertificate
            {
                Username = username,
                PasswordHash = AuthCertificate.CalculateHash(password)
            });
            if (!result.IsCompleted)
            {
                return await Task.FromResult(TaskResult.Uncompleted());
            }
            return await Task.FromResult(TaskResult.FromCondition(result.IsSuccessful));
        }
    }
}
