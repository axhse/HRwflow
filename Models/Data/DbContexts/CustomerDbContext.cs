using Microsoft.EntityFrameworkCore;

namespace HRwflow.Models.Data
{
    public class CustomerDbContext : DatabaseContext<Customer>
    {
        public CustomerDbContext(DbContextOptions<CustomerDbContext> options)
            : base(options) { }

        public CustomerDbContext(string connectionString) : base(connectionString)
        { }

        protected override IEntityTypeConfiguration<Customer> CreateConfiguration()
            => new CustomerConfiguration();
    }
}
