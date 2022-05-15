using System.Collections.Generic;
using System.Linq;

namespace HRwflow.Models
{
    public class MemberProfileVM
    {
        public MemberProfileVM()
        { }

        public MemberProfileVM(Team team,
            string callerUsername, string subjectUsername)
        {
            TeamId = team.TeamId;
            TeamName = team.Properties.Name;
            Username = subjectUsername;
            CallerPermissions = team.Permissions[callerUsername];
            SubjectPermissions = team.Permissions[subjectUsername];
            CanKick = callerUsername != subjectUsername
                && WorkplaceService.CanKick(CallerPermissions, SubjectPermissions);
        }

        public TeamPermissions CallerPermissions { private get; set; }
        public bool CanKick { get; set; }
        public TeamPermissions SubjectPermissions { get; set; }
        public int TeamId { get; set; }
        public string TeamName { get; set; }
        public string Username { get; set; }

        public IEnumerable<TeamPermissions> GetPossibleAppointments()
        {
            var allRoles = new List<TeamPermissions>
            {
                TeamPermissions.Observer,
                TeamPermissions.Commentator,
                TeamPermissions.Editor,
                TeamPermissions.Manager,
                TeamPermissions.Director
            };
            return from role in allRoles
                   where WorkplaceService.CanChangeRole(
                       CallerPermissions, SubjectPermissions, role)
                   select role;
        }
    }
}
