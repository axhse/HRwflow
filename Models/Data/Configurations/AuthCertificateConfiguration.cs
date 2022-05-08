using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRwflow.Models.Data
{
    public class AuthCertificateConfiguration : IEntityTypeConfiguration<AuthCertificate>
    {
        public void Configure(EntityTypeBuilder<AuthCertificate> builder)
        {
            builder.HasKey(certificate => certificate.Username);
            builder.HasIndex(certificate => certificate.Username).IsUnique();
            builder.Property(certificate => certificate.Username).IsRequired();
            builder.Property(certificate => certificate.Username).ValueGeneratedOnAdd();
            builder.Property(certificate => certificate.Username).HasMaxLength(20);
            builder.Property(certificate => certificate.PasswordHash).IsRequired();
            builder.Property(certificate => certificate.PasswordHash).HasMaxLength(64);
            builder.ToTable(nameof(AuthCertificate));
        }
    }
}
