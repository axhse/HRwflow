using Microsoft.EntityFrameworkCore;

namespace HRwflow.Models.Data
{
    public class TeamDbContext : DatabaseContext<Team>
    {
        public TeamDbContext(DbContextOptions<TeamDbContext> options) : base(options)
        { }

        public TeamDbContext(string connectionString) : base(connectionString)
        { }

        protected override IEntityTypeConfiguration<Team> CreateConfiguration()
            => new TeamConfiguration();
    }
}
