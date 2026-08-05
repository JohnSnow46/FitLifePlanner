using FitLifePlanner.Domain.Progress;
using FitLifePlanner.Domain.Workouts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FitLifePlanner.Infrastructure.Configurations;

public class WorkoutLogEntryConfiguration : IEntityTypeConfiguration<WorkoutLogEntry>
{
    public void Configure(EntityTypeBuilder<WorkoutLogEntry> builder)
    {
        builder.HasKey(e => e.Id);

        builder.HasOne<WorkoutLog>()
            .WithMany()
            .HasForeignKey(e => e.WorkoutLogId);

        builder.HasOne<Exercise>()
            .WithMany()
            .HasForeignKey(e => e.ExerciseId);
    }
}
