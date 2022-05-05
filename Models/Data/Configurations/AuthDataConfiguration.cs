using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRwflow.Models.Data
{
    public class AuthDataConfiguration : IEntityTypeConfiguration<AuthData>
    {
        public void Configure(EntityTypeBuilder<AuthData> builder)
        {
            builder.HasKey(authData => authData.Username);
            builder.HasIndex(authData => authData.Username).IsUnique();
            builder.Property(authData => authData.Username).IsRequired();
            builder.Property(authData => authData.Username).ValueGeneratedOnAdd();
            builder.Property(authData => authData.Username).HasMaxLength(20);
            builder.Property(authData => authData.PasswordHash).IsRequired();
            builder.Property(authData => authData.PasswordHash).HasMaxLength(64);
            builder.ToTable(nameof(AuthData));
        }
    }
}
