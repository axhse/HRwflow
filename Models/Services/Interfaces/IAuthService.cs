using System.Threading.Tasks;

namespace HRwflow.Models
{
    public interface IAuthService
    {
        Task<TaskResult> Delete(string username);

        Task<TaskResult> SignIn(string username, string password);

        Task<TaskResult> SignUp(string username, string password);
    }
}
