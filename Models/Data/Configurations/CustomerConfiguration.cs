using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRwflow.Models.Data
{
    public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
    {
        public void Configure(EntityTypeBuilder<Customer> builder)
        {
            builder.HasKey(customer => customer.Username);
            builder.HasIndex(customer => customer.Username).IsUnique();
            builder.Property(customer => customer.Username).IsRequired();
            builder.Property(authData => authData.Username).ValueGeneratedOnAdd();
            builder.Property(customer => customer.Username).HasMaxLength(20);
            builder.Property(customer => customer.Properties).IsRequired();
            // Current Length +- 113;
            builder.Property(customer => customer.Properties).HasMaxLength(500);
            builder.Property(customer => customer.Properties)
                   .HasConversion(new JsonConverter<CustomerProperties>());
            builder.ToTable(nameof(Customer));
            builder.ToTable(nameof(Customer));
        }
    }
}
