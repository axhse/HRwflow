using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRwflow.Models.Data
{
    public class VacancyConfiguration : IEntityTypeConfiguration<Vacancy>
    {
        public void Configure(EntityTypeBuilder<Vacancy> builder)
        {
            builder.HasKey(vacancy => vacancy.VacancyId);
            builder.HasIndex(vacancy => vacancy.VacancyId).IsUnique();
            builder.HasIndex(vacancy => vacancy.OwnerTeamId);
            builder.Property(vacancy => vacancy.VacancyId).IsRequired();
            builder.Property(vacancy => vacancy.VacancyId).ValueGeneratedOnAdd();
            builder.Property(vacancy => vacancy.OwnerTeamId).IsRequired();
            builder.Property(vacancy => vacancy.CreationTime).IsRequired();
            builder.Property(vacancy => vacancy.CreationTime)
                   .HasConversion(new DateTimeConverter());
            builder.Property(vacancy => vacancy.Properties).IsRequired();
            builder.Property(vacancy => vacancy.Properties)
                   .HasConversion(new JsonConverter<VacancyProperties>());
            builder.Property(vacancy => vacancy.Notes).IsRequired();
            builder.Property(vacancy => vacancy.Notes)
                   .HasConversion(new JsonConverter<Dictionary<string, string>>());
            builder.ToTable(nameof(Vacancy));
        }
    }
}
