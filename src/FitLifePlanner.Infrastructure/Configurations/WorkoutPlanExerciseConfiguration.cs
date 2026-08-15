using FitLifePlanner.Domain.Workouts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FitLifePlanner.Infrastructure.Configurations;

public class WorkoutPlanExerciseConfiguration : IEntityTypeConfiguration<WorkoutPlanExercise>
{
    public void Configure(EntityTypeBuilder<WorkoutPlanExercise> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.TargetWeight)
            .HasPrecision(10, 2);

        builder.HasOne<WorkoutPlan>()
            .WithMany(w => w.Exercises)
            .HasForeignKey(e => e.WorkoutPlanId);

        // Exercise is a shared catalog entry: deleting it must not silently wipe out
        // a user's WorkoutPlan.Exercises rows (docs/database.md §2) — restrict instead of cascade.
        builder.HasOne<Exercise>()
            .WithMany()
            .HasForeignKey(e => e.ExerciseId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
