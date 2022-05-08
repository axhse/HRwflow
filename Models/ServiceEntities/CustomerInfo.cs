using System.Collections.Generic;

namespace HRwflow.Models
{
    public class CustomerInfo
    {
        private string _username;

        public HashSet<TeamInfo> TeamInfos { get; set; } = new();

        public string Username
        {
            get => _username;
            set
            {
                if (Customer.UsernameIsCorrect(value))
                {
                    _username = value;
                }
            }
        }
    }
}
