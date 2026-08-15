using FitLifePlanner.Domain.Progress;
using FitLifePlanner.Domain.Workouts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FitLifePlanner.Infrastructure.Configurations;

public class WorkoutLogConfiguration : IEntityTypeConfiguration<WorkoutLog>
{
    public void Configure(EntityTypeBuilder<WorkoutLog> builder)
    {
        builder.HasKey(w => w.Id);

        builder.Property(w => w.Notes)
            .IsRequired()
            .HasMaxLength(2000);

        builder.HasIndex(w => w.UserId);

        // WorkoutPlanId is a nullable FK: deleting a WorkoutPlan must not delete
        // or block-delete historical WorkoutLog rows (docs/database.md §2).
        builder.HasOne<WorkoutPlan>()
            .WithMany()
            .HasForeignKey(w => w.WorkoutPlanId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
