using FitLifePlanner.Domain.Progress;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FitLifePlanner.Infrastructure.Configurations;

public class BodyMetricEntryConfiguration : IEntityTypeConfiguration<BodyMetricEntry>
{
    public void Configure(EntityTypeBuilder<BodyMetricEntry> builder)
    {
        builder.HasKey(b => b.Id);

        builder.Property(b => b.Weight)
            .HasPrecision(10, 2);

        builder.Property(b => b.BodyFatPercent)
            .HasPrecision(5, 2);

        builder.Property(b => b.Notes)
            .IsRequired()
            .HasMaxLength(2000);

        builder.HasIndex(b => b.UserId);
    }
}
