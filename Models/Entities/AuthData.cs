using System.Security.Cryptography;
using System.Text;

namespace HRwflow.Models
{
    public class AuthData
    {
        public string PasswordHash { get; set; }
        public string Username { get; set; }

        public static string GetPasswordHash(string password)
        {
            return Encoding.ASCII.GetString(SHA256.HashData(Encoding.UTF8.GetBytes(password)));
        }
    }
}
