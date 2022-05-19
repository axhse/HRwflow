using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRwflow.Models.Data
{
    public class AuthInfoConfiguration : IEntityTypeConfiguration<AuthInfo>
    {
        public void Configure(EntityTypeBuilder<AuthInfo> builder)
        {
            builder.HasKey(info => info.Username);
            builder.HasIndex(info => info.Username).IsUnique();
            builder.Property(info => info.Username).IsRequired();
            builder.Property(info => info.Username).ValueGeneratedNever();
            builder.Property(info => info.Username).HasMaxLength(20);
            builder.Property(info => info.PasswordHash).IsRequired();
            builder.Property(info => info.PasswordHash).HasMaxLength(64);
            builder.ToTable(nameof(AuthInfo));
        }
    }
}
