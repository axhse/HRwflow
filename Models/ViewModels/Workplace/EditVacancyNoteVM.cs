namespace HRwflow.Models
{
    public class EditVacancyNoteVM
    {
        public bool CanDelete { get; set; } = false;
        public bool CanEdit { get; set; } = false;
        public bool HasErrors => !IsTextCorrect;
        public bool IsTextCorrect { get; set; } = true;
        public VacancyNote Note { get; set; }
        public int VacancyId { get; set; }
    }
}
