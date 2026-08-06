using FitLifePlanner.Domain.Progress;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FitLifePlanner.Infrastructure.Configurations;

public class BodyMetricEntryConfiguration : IEntityTypeConfiguration<BodyMetricEntry>
{
    public void Configure(EntityTypeBuilder<BodyMetricEntry> builder)
    {
        builder.HasKey(b => b.Id);

        builder.Property(b => b.Notes)
            .IsRequired()
            .HasMaxLength(2000);
    }
}
