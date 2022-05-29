namespace HRwflow.Models
{
    public class VacancyVM
    {
        public Team Team { get; set; }
        public string Username { get; set; }
        public Vacancy Vacancy { get; set; }

        public bool CanDelete
            => Team.MemberHasPermissions(Username,
                TeamPermissions.DeleteVacancy);

        public bool CanEdit
            => Team.MemberHasPermissions(Username,
                TeamPermissions.ModifyVacancy);

        public bool CanComment
            => Team.MemberHasPermissions(Username,
                TeamPermissions.CommentVacancy);
    }
}
