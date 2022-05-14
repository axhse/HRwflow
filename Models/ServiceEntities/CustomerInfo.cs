using System.Collections.Generic;

namespace HRwflow.Models
{
    public enum AccountStates
    {
        Active,
        OnDeletion
    }

    public class CustomerInfo
    {
        private string _username;

        public AccountStates AccountState { get; set; } = AccountStates.Active;

        public Dictionary<int, string> JoinedTeamNames { get; set; } = new();

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
