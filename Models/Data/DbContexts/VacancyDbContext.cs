using Microsoft.EntityFrameworkCore;

namespace HRwflow.Models.Data
{
    public class VacancyDbContext : DatabaseContext<Vacancy>
    {
        public VacancyDbContext(DbContextOptions<VacancyDbContext> options)
            : base(options) { }

        public VacancyDbContext(string connectionString) : base(connectionString)
        { }

        protected override IEntityTypeConfiguration<Vacancy> CreateConfiguration()
            => new VacancyConfiguration();
    }
}
