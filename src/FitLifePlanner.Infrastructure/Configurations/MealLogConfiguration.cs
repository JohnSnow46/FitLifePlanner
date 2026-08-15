using FitLifePlanner.Domain.Nutrition;
using FitLifePlanner.Domain.Progress;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FitLifePlanner.Infrastructure.Configurations;

public class MealLogConfiguration : IEntityTypeConfiguration<MealLog>
{
    public void Configure(EntityTypeBuilder<MealLog> builder)
    {
        builder.HasKey(m => m.Id);

        builder.Property(m => m.QuantityConsumed)
            .HasPrecision(10, 2);

        builder.HasIndex(m => m.UserId);

        // Food is a shared catalog entry: deleting it must not silently wipe out
        // historical MealLog rows (docs/database.md §2) — restrict instead of cascade.
        builder.HasOne<Food>()
            .WithMany()
            .HasForeignKey(m => m.FoodId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
