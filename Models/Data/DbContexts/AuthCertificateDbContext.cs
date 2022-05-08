using Microsoft.EntityFrameworkCore;

namespace HRwflow.Models.Data
{
    public class AuthCertificateDbContext : DatabaseContext<AuthCertificate>
    {
        public AuthCertificateDbContext(DbContextOptions<AuthCertificateDbContext> options) : base(options)
        { }

        public AuthCertificateDbContext(string connectionString) : base(connectionString)
        { }

        protected override IEntityTypeConfiguration<AuthCertificate> CreateConfiguration()
            => new AuthCertificateConfiguration();
    }
}
