using Microsoft.EntityFrameworkCore;

namespace HRwflow.Models.Data
{
    public class AuthInfoDbContext : DatabaseContext<AuthInfo>
    {
        public AuthInfoDbContext(DbContextOptions<AuthInfoDbContext> options)
            : base(options) { }

        public AuthInfoDbContext(string connectionString) : base(connectionString)
        { }

        protected override IEntityTypeConfiguration<AuthInfo> CreateConfiguration()
            => new AuthInfoConfiguration();
    }
}
