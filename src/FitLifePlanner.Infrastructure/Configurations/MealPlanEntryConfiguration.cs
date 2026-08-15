using FitLifePlanner.Domain.Nutrition;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FitLifePlanner.Infrastructure.Configurations;

public class MealPlanEntryConfiguration : IEntityTypeConfiguration<MealPlanEntry>
{
    public void Configure(EntityTypeBuilder<MealPlanEntry> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Quantity)
            .HasPrecision(10, 2);

        builder.HasOne<MealPlan>()
            .WithMany(p => p.Entries)
            .HasForeignKey(e => e.MealPlanId);

        // Food is a shared catalog entry: deleting it must not silently wipe out
        // a user's MealPlan.Entries rows (docs/database.md §2) — restrict instead of cascade.
        builder.HasOne<Food>()
            .WithMany()
            .HasForeignKey(e => e.FoodId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
