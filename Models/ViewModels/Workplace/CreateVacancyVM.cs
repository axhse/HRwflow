namespace HRwflow.Models
{
    public class CreateVacancyVM
    {
        public VacancyCreationErrors Error
        {
            set => CanCreate = false;
        }

        public bool HasErrors => !IsTitleCorrect || !CanCreate;
        public bool IsTitleCorrect { get; set; } = true;
        public bool CanCreate { get; set; } = true;
        public VacancyProperties Properties { get; set; } = new();
    }
}
