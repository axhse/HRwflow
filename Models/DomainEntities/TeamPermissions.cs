using System;

namespace HRwflow.Models
{
    [Flags]
    public enum TeamPermissions
    {
        None = 0,

        CommentVacancy = 1,
        ManageVacancyNotes = 1 << 1,
        CreateVacancy = 1 << 2,
        DeleteVacancy = 1 << 3,
        ModifyVacancy = 1 << 4,
        Invite = 1 << 5,
        KickMember = 1 << 6,
        ModifyMemberPermissions = 1 << 7,
        PromoteToManager = 1 << 8,
        DemoteFromManager = 1 << 9,
        KickManager = 1 << 10,
        ModifyManagerPermissions = 1 << 11,
        PromoteToDirector = 1 << 12,
        DemoteFromDirector = 1 << 13,
        ModifyDirectorPermissions = 1 << 14,
        KickDirector = 1 << 15,
        ModifyTeamProperties = 1 << 16,

        All = (1 << 17) - 1,
        Observer = None,
        Commentator = CommentVacancy,

        Editor = Commentator + CreateVacancy + DeleteVacancy
            + ModifyVacancy + ManageVacancyNotes,

        Manager = Editor + Invite + KickMember + ModifyMemberPermissions,

        Director = All - DemoteFromDirector
            - ModifyDirectorPermissions - KickDirector,
    }
}
