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

        builder.Property(e => e.WeightUsed)
            .HasPrecision(10, 2);

        builder.HasOne<WorkoutLog>()
            .WithMany(l => l.Entries)
            .HasForeignKey(e => e.WorkoutLogId);

        // Exercise is a shared catalog entry: deleting it must not silently wipe out
        // historical WorkoutLogEntry rows (docs/database.md §2) — restrict instead of cascade.
        builder.HasOne<Exercise>()
            .WithMany()
            .HasForeignKey(e => e.ExerciseId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
