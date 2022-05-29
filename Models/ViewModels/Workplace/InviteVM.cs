namespace HRwflow.Models
{
    public class InviteVM
    {
        private InvitationErrors _error;

        public InvitationErrors Error
        {
            get => _error;
            set
            {
                _error = value;
                HasError = true;
            }
        }

        public int TeamId { get; set; }

        public bool HasError { get; private set; } = false;
    }
}
