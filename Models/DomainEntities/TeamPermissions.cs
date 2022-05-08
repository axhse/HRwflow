using System;

namespace HRwflow.Models
{
    [Flags]
    public enum TeamPermissions
    {
        None = 0,
        Direct = 1,
        Manage = 2,
        ModifyTeamProperties = 4,
        CreateVacancy = 8,
        DeleteVacancy = 16,
        ModifyVacancyProperties = 32,
        All = 64 - 1
    }
}
