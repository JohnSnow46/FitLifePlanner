using FitLifePlanner.Domain.Nutrition;
using FitLifePlanner.Domain.Progress;
using FitLifePlanner.Domain.Users;
using FitLifePlanner.Domain.Workouts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace FitLifePlanner.Infrastructure.Data;

public class FitLifePlannerDbContext(DbContextOptions<FitLifePlannerDbContext> options)
    : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Exercise> Exercises => Set<Exercise>();
    public DbSet<WorkoutPlan> WorkoutPlans => Set<WorkoutPlan>();
    public DbSet<WorkoutPlanExercise> WorkoutPlanExercises => Set<WorkoutPlanExercise>();
    public DbSet<Food> Foods => Set<Food>();
    public DbSet<MealPlan> MealPlans => Set<MealPlan>();
    public DbSet<MealPlanEntry> MealPlanEntries => Set<MealPlanEntry>();
    public DbSet<WorkoutLog> WorkoutLogs => Set<WorkoutLog>();
    public DbSet<WorkoutLogEntry> WorkoutLogEntries => Set<WorkoutLogEntry>();
    public DbSet<MealLog> MealLogs => Set<MealLog>();
    public DbSet<BodyMetricEntry> BodyMetricEntries => Set<BodyMetricEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(FitLifePlannerDbContext).Assembly);

        // SQLite has no DateTime kind of its own: values come back from storage as
        // DateTimeKind.Unspecified. Force UTC on read so every DateTime leaving the API
        // serializes with a "Z" suffix (docs/api.md §1: all wire DateTimes are UTC).
        // On write, actually convert offset/local values to UTC instead of just tagging
        // them — otherwise an incoming "+02:00" timestamp is stored as if it were UTC,
        // silently shifting it. A value with no offset at all (Kind=Unspecified) is
        // treated as already-UTC (SpecifyKind, no conversion), matching prior behavior
        // for dates that never carried a timezone.
        var utcConverter = new ValueConverter<DateTime, DateTime>(
            toProvider => toProvider.Kind == DateTimeKind.Unspecified
                ? DateTime.SpecifyKind(toProvider, DateTimeKind.Utc)
                : toProvider.ToUniversalTime(),
            fromProvider => DateTime.SpecifyKind(fromProvider, DateTimeKind.Utc));

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                if (property.ClrType == typeof(DateTime))
                {
                    property.SetValueConverter(utcConverter);
                }
            }
        }
    }
}
