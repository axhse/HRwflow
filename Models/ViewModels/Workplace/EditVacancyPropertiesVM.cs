namespace HRwflow.Models
{
    public class EditVacancyPropertiesVM
    {
        public EditVacancyPropertiesVM(
            VacancyProperties properties = default)
        {
            VacancyProperties = properties;
        }

        public bool HasErrors => !IsTitleCorrect || !IsDescriptionCorrect;
        public bool IsDescriptionCorrect { get; set; } = true;
        public bool IsTitleCorrect { get; set; } = true;
        public int VacancyId { get; set; }
        public VacancyProperties VacancyProperties { get; set; }
    }
}
