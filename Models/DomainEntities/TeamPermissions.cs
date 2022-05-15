using System;

namespace HRwflow.Models
{
    [Flags]
    public enum TeamPermissions
    {
        None = 0,

        ModifyTeamProperties = 1,
        CreateVacancy = 1 << 1,
        DeleteVacancy = 1 << 2,
        ModifyVacancy = 1 << 3,
        CommentVacancy = 1 << 4,
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

        All = (1 << 16) - 1,
        Observer = None,
        Commentator = CommentVacancy,
        Editor = Commentator + CreateVacancy + DeleteVacancy + ModifyVacancy,
        Manager = Editor + Invite + KickMember + ModifyMemberPermissions,

        Director = All - DemoteFromDirector
            - ModifyDirectorPermissions - KickDirector,
    }
}
