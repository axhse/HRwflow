namespace HRwflow.Models
{
    public enum SignInErrors
    {
        AccountNotFound,
        PasswordIsWrong
    }

    public enum SignUpErrors
    {
        UsernameIsTaken
    }

    public enum InvitationErrors
    {
        UserNotFound,
        UserAlreadyJoined,
        JoinLimitExceeded,
        TeamSizeLimitExceeded
    }

    public enum TeamCreationErrors
    {
        JoinLimitExceeded
    }

    public enum VacancyCreationErrors
    {
        VacancyCountLimitExceeded
    }

    public enum CommonErrors
    {
        NoPermission,
        ResourceNotFound
    }
}
