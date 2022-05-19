using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRwflow.Models.Data
{
    public class CustomerInfoConfiguration : IEntityTypeConfiguration<CustomerInfo>
    {
        public void Configure(EntityTypeBuilder<CustomerInfo> builder)
        {
            builder.HasKey(info => info.Username);
            builder.HasIndex(info => info.Username).IsUnique();
            builder.Property(info => info.Username).IsRequired();
            builder.Property(info => info.Username).ValueGeneratedNever();
            builder.Property(info => info.Username).HasMaxLength(20);
            builder.Property(info => info.AccountState).IsRequired();
            builder.Property(info => info.JoinedTeamNames).IsRequired();
            builder.Property(info => info.JoinedTeamNames)
                   .HasConversion(new JsonConverter<Dictionary<int, string>>());
            builder.ToTable(nameof(CustomerInfo));
        }
    }
}
