namespace HRwflow.Models
{
    public class EditVacancyPropertiesVM
    {
        public EditVacancyPropertiesVM(
            VacancyProperties properties = default)
        {
            VacancyProperties = properties;
        }

        public bool DescriptionIsCorrect { get; set; } = true;
        public bool HasErrors => !TitleIsCorrect || !DescriptionIsCorrect;
        public bool TitleIsCorrect { get; set; } = true;
        public int VacancyId { get; set; }
        public VacancyProperties VacancyProperties { get; set; }
    }
}
