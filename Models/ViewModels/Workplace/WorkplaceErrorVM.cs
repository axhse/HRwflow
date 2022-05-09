namespace HRwflow.Models
{
    public enum WorkplaceErrors
    {
        None,
        ServerError,
        ResourceNotFound,

        JoinLimitExceeded,
        TeamSizeLimitExceeded,
        VacancyCountLimitExceeded,

        NoPermission
    }

    public class WorkplaceErrorVM
    {
        public WorkplaceErrorVM(WorkplaceErrors error = WorkplaceErrors.None)
        {
            Error = error;
        }

        public WorkplaceErrors Error { get; set; }
    }
}
