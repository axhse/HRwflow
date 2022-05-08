using Microsoft.EntityFrameworkCore;

namespace HRwflow.Models.Data
{
    public class CustomerInfoDbContext : DatabaseContext<CustomerInfo>
    {
        public CustomerInfoDbContext(DbContextOptions<CustomerInfoDbContext> options) : base(options)
        { }

        public CustomerInfoDbContext(string connectionString) : base(connectionString)
        { }

        protected override IEntityTypeConfiguration<CustomerInfo> CreateConfiguration()
            => new CustomerInfoConfiguration();
    }
}
