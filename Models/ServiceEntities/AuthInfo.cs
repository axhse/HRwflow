using System.Security.Cryptography;
using System.Text;

namespace HRwflow.Models
{
    public class AuthInfo
    {
        public string PasswordHash { get; set; }
        public string Username { get; set; }

        public static string CalculateHash(string password)
        {
            return Encoding.ASCII.GetString(SHA256.HashData(Encoding.UTF8.GetBytes(password)));
        }

        public static bool IsPasswordCorrect(string password) => password != null
                && 8 <= password.Length && password.Length <= 40;
    }
}
