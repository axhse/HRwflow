namespace HRwflow.Models
{
    public class CreateTeamVM
    {
        public TeamCreationErrors Error
        {
            set => CanCreate = false;
        }

        public bool HasErrors => !IsNameCorrect || !CanCreate;
        public bool IsNameCorrect { get; set; } = true;
        public bool CanCreate { get; set; } = true;
        public TeamProperties Properties { get; set; } = new();
    }
}
