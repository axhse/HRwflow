﻿using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRwflow.Models.Data
{
    public class TeamConfiguration : IEntityTypeConfiguration<Team>
    {
        public void Configure(EntityTypeBuilder<Team> builder)
        {
            builder.HasKey(team => team.TeamId);
            builder.HasIndex(team => team.TeamId).IsUnique();
            builder.Property(team => team.TeamId).IsRequired();
            builder.Property(team => team.TeamId).ValueGeneratedOnAdd();
            builder.Property(team => team.VacancyCount).IsRequired();
            builder.Property(team => team.Properties).IsRequired();
            builder.Property(team => team.Properties)
                   .HasConversion(new JsonConverter<TeamProperties>());
            builder.Property(team => team.Permissions).IsRequired();
            builder.Property(team => team.Permissions).HasConversion(
                new JsonConverter<Dictionary<string, TeamPermissions>>());
            builder.ToTable(nameof(Team));
        }
    }
}
