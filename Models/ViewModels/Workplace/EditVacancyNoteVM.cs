namespace HRwflow.Models
{
    public class EditVacancyNoteVM
    {
        public bool CanDelete { get; set; } = false;
        public bool CanEdit { get; set; } = false;
        public bool HasErrors => !TextIsCorrect;
        public VacancyNote Note { get; set; }
        public bool TextIsCorrect { get; set; } = true;
        public int VacancyId { get; set; }
    }
}
