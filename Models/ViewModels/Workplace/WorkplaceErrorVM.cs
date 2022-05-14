namespace HRwflow.Models
{
    public enum WorkplaceErrors
    {
        None,
        ServerError,

        NoPermission,
        ResourceNotFound,
        UserNotFound,
        UserAlreadyJoined,

        JoinLimitExceeded,
        TeamSizeLimitExceeded,
        VacancyCountLimitExceeded
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
