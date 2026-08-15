using FitLifePlanner.Domain.Nutrition;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FitLifePlanner.Infrastructure.Configurations;

public class FoodConfiguration : IEntityTypeConfiguration<Food>
{
    public void Configure(EntityTypeBuilder<Food> builder)
    {
        builder.HasKey(f => f.Id);

        builder.Property(f => f.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(f => f.Unit)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(f => f.CaloriesPerUnit)
            .HasPrecision(10, 2);

        builder.Property(f => f.ProteinPerUnit)
            .HasPrecision(10, 2);

        builder.Property(f => f.CarbsPerUnit)
            .HasPrecision(10, 2);

        builder.Property(f => f.FatPerUnit)
            .HasPrecision(10, 2);
    }
}
