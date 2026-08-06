using FitLifePlanner.Domain.Nutrition;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FitLifePlanner.Infrastructure.Configurations;

public class MealPlanEntryConfiguration : IEntityTypeConfiguration<MealPlanEntry>
{
    public void Configure(EntityTypeBuilder<MealPlanEntry> builder)
    {
        builder.HasKey(e => e.Id);

        builder.HasOne<MealPlan>()
            .WithMany(p => p.Entries)
            .HasForeignKey(e => e.MealPlanId);

        builder.HasOne<Food>()
            .WithMany()
            .HasForeignKey(e => e.FoodId);
    }
}
