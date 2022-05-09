namespace HRwflow.Models
{
    public interface IAuthService
    {
        TaskResult DeleteAccount(string username);

        TaskResult<bool> IsUserExists(string username);

        TaskResult<bool> SignIn(string username, string password);

        TaskResult SignUp(string username, string password);

        TaskResult UpdatePassword(string username, string password);
    }
}
