using System;

namespace HRwflow.Models
{
    [Flags]
    public enum TeamPermissions
    {
        None = 0,
        ModifyTeamProperties = 1,
        CreateVacancy = 2,
        DeleteVacancy = 4,
        ModifyVacancyProperties = 8,
        Manage = 16,
        Direct = 32 + (32 - 1)
    }
}
