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

        builder.HasOne<Food>()
            .WithMany()
            .HasForeignKey(m => m.FoodId);
    }
}
