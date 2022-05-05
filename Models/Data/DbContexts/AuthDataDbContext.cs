using Microsoft.EntityFrameworkCore;

namespace HRwflow.Models.Data
{
    public class AuthDataDbContext : DatabaseContext<AuthData>
    {
        public AuthDataDbContext(DbContextOptions<AuthDataDbContext> options) : base(options)
        { }

        public AuthDataDbContext(string connectionString) : base(connectionString)
        { }

        protected override IEntityTypeConfiguration<AuthData> CreateConfiguration()
            => new AuthDataConfiguration();
    }
}
