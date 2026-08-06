using FitLifePlanner.Domain.Workouts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FitLifePlanner.Infrastructure.Configurations;

public class WorkoutPlanExerciseConfiguration : IEntityTypeConfiguration<WorkoutPlanExercise>
{
    public void Configure(EntityTypeBuilder<WorkoutPlanExercise> builder)
    {
        builder.HasKey(e => e.Id);

        builder.HasOne<WorkoutPlan>()
            .WithMany(w => w.Exercises)
            .HasForeignKey(e => e.WorkoutPlanId);

        builder.HasOne<Exercise>()
            .WithMany()
            .HasForeignKey(e => e.ExerciseId);
    }
}
